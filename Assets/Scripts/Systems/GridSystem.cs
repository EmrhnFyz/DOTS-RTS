#define GRID_DEBUG
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public partial struct GridSystem : ISystem
{
	public const int WALL_COST = byte.MaxValue;
	public const int FLOW_FIELD_MAP_COUNT = 150;

	public struct GridSystemData : IComponentData
	{
		public int Width;
		public int Height;
		public int NextAvailableMapIndex;

		public float CellSize;

		public NativeArray<GridMap> GridMapArray;
	}

	public struct GridMap
	{
		public NativeArray<Entity> GridEntityArray;
	}

	public struct GridNode : IComponentData
	{
		public int Index;
		public int X;
		public int Y;

		public byte Cost;
		public byte BestCost;

		public float2 Vector;
	}

	public static int CalculateIndex(int2 gridPosition, int width) => CalculateIndex(gridPosition.x, gridPosition.y, width);
	public static int CalculateIndex(int x, int y, int width) => x + y * width;
	public static float2 CalculateVector(int fromX, int fromY, int toX, int toY) => new float2(toX, toY) - new float2(fromX, fromY);

	public static float3 GetWorldPosition(int x, int y, float cellSize) => new(x * cellSize, 0, y * cellSize);
	public static float3 GetWorldCenterPosition(int x, int y, float cellSize) => new(x * cellSize + cellSize * 0.5f, 0, y * cellSize + cellSize * 0.5f);

	public static bool IsValidGridPosition(int2 gridPosition, int width, int height) => gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
	public static float3 GetWorldMovementVector(float2 vector) => new(vector.x, 0, vector.y);
	public static bool IsWall(GridNode gridNode) => gridNode.Cost == WALL_COST;

	public static int2 GetGridPosition(float3 worldPosition, float cellSize)
	{
		var x = (int)math.floor(worldPosition.x / cellSize);
		var y = (int)math.floor(worldPosition.z / cellSize);
		return new int2(x, y);
	}


	public static NativeList<RefRW<GridNode>> GetNeighbourGridNodeList(RefRW<GridNode> currentGridNode, NativeArray<RefRW<GridNode>> gridNodeNativeArray, int width, int height)
	{
		var neighbourGridNodeList = new NativeList<RefRW<GridNode>>(Allocator.Temp);

		var gridNodeX = currentGridNode.ValueRO.X;
		var gridNodeY = currentGridNode.ValueRO.Y;
		var directions = new[]
		                 {
			                 new int2(-1, 0), // Left
			                 new int2(1, 0), // Right
			                 new int2(0, 1), // Up
			                 new int2(0, -1), // Down
			                 new int2(-1, -1), // Lower Left
			                 new int2(1, -1), // Lower Right
			                 new int2(-1, 1), // Upper Left
			                 new int2(1, 1) // Upper Right
		                 };

		foreach (var direction in directions)
		{
			var neighborPos = new int2(gridNodeX + direction.x, gridNodeY + direction.y);
			if (IsValidGridPosition(neighborPos, width, height))
			{
				neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(neighborPos, width)]);
			}
		}

		return neighbourGridNodeList;
	}

#if !GRID_DEBUG
	[BurstCompile]
#endif
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
		var width = 50;
		var height = 50;
		var gridCellSize = 1.5f;
		var totalCells = width * height;
		var gridNodeEntityPrefab = state.EntityManager.CreateEntity();
		state.EntityManager.AddComponent<GridNode>(gridNodeEntityPrefab);

		var gridMapArray = new NativeArray<GridMap>(FLOW_FIELD_MAP_COUNT, Allocator.Persistent);
		for (var i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
		{
			var gridMap = new GridMap();
			gridMap.GridEntityArray = new NativeArray<Entity>(totalCells, Allocator.Persistent);

			state.EntityManager.Instantiate(gridNodeEntityPrefab, gridMap.GridEntityArray);

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					var index = CalculateIndex(x, y, width);
					var gridNode = new GridNode
					               {
						               Index = index,
						               X = x,
						               Y = y
					               };
#if GRID_DEBUG
					state.EntityManager.SetName(gridMap.GridEntityArray[index], $"GridNode_{x}_{y}");
#endif
					SystemAPI.SetComponent(gridMap.GridEntityArray[index], gridNode);
				}
			}

			gridMapArray[i] = gridMap;
		}

		state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
		state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
		                                                         {
			                                                         Width = width,
			                                                         Height = height,
			                                                         CellSize = gridCellSize,
			                                                         GridMapArray = gridMapArray
		                                                         });
	}
#if !GRID_DEBUG
	[BurstCompile]
#endif
	public void OnUpdate(ref SystemState state)
	{
		var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

		foreach (var (fieldPathRequest, flowFieldPathRequestEnabled, flowFieldFollower, flowFieldFollowerEnabled)
		         in SystemAPI.Query<RefRW<FlowFieldPathRequest>, EnabledRefRW<FlowFieldPathRequest>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldFollower>())
		{
			var targetGridPosition = GetGridPosition(fieldPathRequest.ValueRO.TargetPosition, gridSystemData.CellSize);

			flowFieldPathRequestEnabled.ValueRW = false;

			var gridIndex = gridSystemData.NextAvailableMapIndex;
			gridSystemData.NextAvailableMapIndex = (gridSystemData.NextAvailableMapIndex + 1) % FLOW_FIELD_MAP_COUNT;
			SystemAPI.SetComponent(state.SystemHandle, gridSystemData);

			flowFieldFollower.ValueRW.GridIndex = gridIndex;
			flowFieldFollower.ValueRW.TargetPosition = fieldPathRequest.ValueRO.TargetPosition;
			flowFieldFollowerEnabled.ValueRW = true;

			var gridNodeNativeArray = new NativeArray<RefRW<GridNode>>(gridSystemData.Width * gridSystemData.Height, Allocator.Temp);
			for (var x = 0; x < gridSystemData.Width; x++)
			{
				for (var y = 0; y < gridSystemData.Height; y++)
				{
					var index = CalculateIndex(x, y, gridSystemData.Width);
					var gridNodeEntity = gridSystemData.GridMapArray[gridIndex].GridEntityArray[index];
					var gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);

					gridNodeNativeArray[index] = gridNode;

					gridNode.ValueRW.Vector = new float2(0, 1);
					if (x == targetGridPosition.x && y == targetGridPosition.y)
					{
						// target destination

						gridNode.ValueRW.Cost = 0;
						gridNode.ValueRW.BestCost = 0;
					}
					else
					{
						gridNode.ValueRW.Cost = 1;
						gridNode.ValueRW.BestCost = byte.MaxValue;
					}
				}
			}


			var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
			var collisionWorld = physicsWorldSingleton.PhysicsWorld.CollisionWorld;

			var distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

			for (var x = 0; x < gridSystemData.Width; x++)
			{
				for (var y = 0; y < gridSystemData.Height; y++)
				{
					if (collisionWorld.OverlapSphere(GetWorldCenterPosition(x, y, gridSystemData.CellSize),
					                                 gridSystemData.CellSize * 0.5f,
					                                 ref distanceHitList,
					                                 new CollisionFilter
					                                 {
						                                 BelongsTo = ~0u,
						                                 CollidesWith = 1u << GameConfig.PATHFINDING_WALLS,
						                                 GroupIndex = 0
					                                 }))
					{
						var index = CalculateIndex(x, y, gridSystemData.Width);
						gridNodeNativeArray[index].ValueRW.Cost = WALL_COST;
					}
				}
			}

			distanceHitList.Dispose();

			var gridNodeOpenQueue = new NativeQueue<RefRW<GridNode>>(Allocator.Temp);

			var targetGridNode = gridNodeNativeArray[CalculateIndex(targetGridPosition, gridSystemData.Width)];

			gridNodeOpenQueue.Enqueue(targetGridNode);

			while (gridNodeOpenQueue.Count > 0)
			{
				var currentGridNode = gridNodeOpenQueue.Dequeue();

				var neighbourGridNodeList = GetNeighbourGridNodeList(currentGridNode, gridNodeNativeArray, gridSystemData.Width, gridSystemData.Height);

				foreach (var neighborGridNode in neighbourGridNodeList)
				{
					if (neighborGridNode.ValueRO.Cost == WALL_COST)
					{
						continue; // Skip walls
					}

					var newMaxCost = (byte)(currentGridNode.ValueRW.BestCost + neighborGridNode.ValueRO.Cost);

					if (newMaxCost < neighborGridNode.ValueRO.BestCost)
					{
						neighborGridNode.ValueRW.BestCost = newMaxCost;
						neighborGridNode.ValueRW.Vector = CalculateVector(neighborGridNode.ValueRO.X, neighborGridNode.ValueRO.Y, currentGridNode.ValueRO.X, currentGridNode.ValueRO.Y);

						gridNodeOpenQueue.Enqueue(neighborGridNode);
					}
				}

				neighbourGridNodeList.Dispose();
			}

			gridNodeOpenQueue.Dispose();
			gridNodeNativeArray.Dispose();
		}

#if GRID_DEBUG
		GridSystemDebug.Instance?.InitializeGrid(gridSystemData);
		GridSystemDebug.Instance?.UpdateGird(gridSystemData);
#endif
	}

	[BurstCompile]
	public void OnDestroy(ref SystemState state)
	{
		var gridSystemData = SystemAPI.GetComponentRW<GridSystemData>(state.SystemHandle);
		for (var i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
		{
			gridSystemData.ValueRW.GridMapArray[i].GridEntityArray.Dispose();
		}

		gridSystemData.ValueRW.GridMapArray.Dispose();
	}
}