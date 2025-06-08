using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

public partial struct GridSystem : ISystem
{
	public const int WALL_COST = byte.MaxValue;
	public const int HEAVY_COST = 50;

	public const int FLOW_FIELD_MAP_COUNT = 150;

	public struct GridSystemData : IComponentData
	{
		public int Width;
		public int Height;
		public int NextAvailableMapIndex;

		public float CellSize;

		public NativeArray<GridMap> GridMapArray;
		public NativeArray<byte> GridCostArray;

		public NativeArray<Entity> TotalGridMapEntityArray;
	}

	public struct GridMap
	{
		public NativeArray<Entity> GridEntityArray;

		public int2 TargetGridPosition;

		public bool IsValid;
	}

	public struct GridNode : IComponentData
	{
		public int GridIndex;
		public int Index;
		public int X;
		public int Y;

		public byte Cost;
		public int BestCost;

		public float2 Vector;
	}

	public ComponentLookup<GridNode> GridNodeLookup;


	public static int CalculateIndex(int2 gridPosition, int width) => CalculateIndex(gridPosition.x, gridPosition.y, width);
	public static int CalculateIndex(int x, int y, int width) => x + y * width;

	public static int2 GetGridPositionFromIndex(int index, int width) => new(index % width, (int)math.floor(index / width));
	public static float2 CalculateVector(int fromX, int fromY, int toX, int toY) => new float2(toX, toY) - new float2(fromX, fromY);

	public static float3 GetWorldPosition(int x, int y, float cellSize) => new(x * cellSize, 0, y * cellSize);
	public static float3 GetWorldCenterPosition(int x, int y, float cellSize) => new(x * cellSize + cellSize * 0.5f, 0, y * cellSize + cellSize * 0.5f);

	public static bool IsValidGridPosition(int2 gridPosition, int width, int height) => gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
	public static float3 GetWorldMovementVector(float2 vector) => new(vector.x, 0, vector.y);
	public static bool IsWall(GridNode gridNode) => gridNode.Cost == WALL_COST;
	public static bool IsWall(int2 gridPosition, GridSystemData gridSystemData) => gridSystemData.GridCostArray[CalculateIndex(gridPosition, gridSystemData.Width)] == WALL_COST;
	public static bool IsWall(int2 gridPosition, int width, NativeArray<byte> gridCostArray) => gridCostArray[CalculateIndex(gridPosition, width)] == WALL_COST;

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
		var positionLeft = new int2(gridNodeX - 1, gridNodeY + 0);
		var positionRight = new int2(gridNodeX + 1, gridNodeY + 0);
		var positionUp = new int2(gridNodeX + 0, gridNodeY + 1);
		var positionDown = new int2(gridNodeX + 0, gridNodeY - 1);

		var positionLowerLeft = new int2(gridNodeX - 1, gridNodeY - 1);
		var positionLowerRight = new int2(gridNodeX + 1, gridNodeY - 1);
		var positionUpperLeft = new int2(gridNodeX - 1, gridNodeY + 1);
		var positionUpperRight = new int2(gridNodeX + 1, gridNodeY + 1);

		if (IsValidGridPosition(positionLeft, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionLeft, width)]);
		}
		if (IsValidGridPosition(positionRight, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionRight, width)]);
		}
		if (IsValidGridPosition(positionUp, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionUp, width)]);
		}
		if (IsValidGridPosition(positionDown, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionDown, width)]);
		}

		if (IsValidGridPosition(positionLowerLeft, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionLowerLeft, width)]);
		}
		if (IsValidGridPosition(positionLowerRight, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionLowerRight, width)]);
		}
		if (IsValidGridPosition(positionUpperLeft, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionUpperLeft, width)]);
		}
		if (IsValidGridPosition(positionUpperRight, width, height))
		{
			neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionUpperRight, width)]);
		}

		return neighbourGridNodeList;
	}

	public static bool IsValidWalkablePosition(float3 worldPosition, GridSystemData gridSystemData)
	{
		var gridPosition = GetGridPosition(worldPosition, gridSystemData.CellSize);

		return IsValidGridPosition(gridPosition, gridSystemData.Width, gridSystemData.Height) && !IsWall(gridPosition, gridSystemData);
	}

	public static bool IsValidWalkablePosition(float3 worldPosition, int width, int height, NativeArray<byte> gridCostArray, float cellSize)
	{
		var gridPosition = GetGridPosition(worldPosition, cellSize);
		return IsValidGridPosition(gridPosition, width, height) && !IsWall(gridPosition, width, gridCostArray);
	}

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
		var width = 50;
		var height = 50;
		var gridCellSize = 1.5f;
		var totalCells = width * height;
		var gridNodeEntityPrefab = state.EntityManager.CreateEntity();
		var totalGridMapEntityList = new NativeList<Entity>(FLOW_FIELD_MAP_COUNT * totalCells, Allocator.Persistent);
		state.EntityManager.AddComponent<GridNode>(gridNodeEntityPrefab);
		var gridMapArray = new NativeArray<GridMap>(FLOW_FIELD_MAP_COUNT, Allocator.Persistent);
		for (var i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
		{
			var gridMap = new GridMap
			{
				IsValid = false,
				GridEntityArray = new NativeArray<Entity>(totalCells, Allocator.Persistent)
			};

			state.EntityManager.Instantiate(gridNodeEntityPrefab, gridMap.GridEntityArray);
			totalGridMapEntityList.AddRange(gridMap.GridEntityArray);
			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					var index = CalculateIndex(x, y, width);
					var gridNode = new GridNode
					{
						GridIndex = i,
						Index = index,
						X = x,
						Y = y
					};
					state.EntityManager.SetName(gridMap.GridEntityArray[index], $"GridNode_{x}_{y}");
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
			GridMapArray = gridMapArray,
			GridCostArray = new NativeArray<byte>(totalCells, Allocator.Persistent),
			TotalGridMapEntityArray = totalGridMapEntityList.ToArray(Allocator.Persistent)
		});
		totalGridMapEntityList.Dispose();
		GridNodeLookup = SystemAPI.GetComponentLookup<GridNode>(false);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);
		GridNodeLookup.Update(ref state);

		foreach (var (fieldPathRequest, flowFieldPathRequestEnabled, flowFieldFollower, flowFieldFollowerEnabled)
				 in SystemAPI.Query<RefRW<FlowFieldPathRequest>, EnabledRefRW<FlowFieldPathRequest>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldFollower>())
		{
			var targetGridPosition = GetGridPosition(fieldPathRequest.ValueRO.TargetPosition, gridSystemData.CellSize);

			flowFieldPathRequestEnabled.ValueRW = false;
			bool alreadyCalculatedPath = false;

			for (int i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
			{
				if (gridSystemData.GridMapArray[i].IsValid && gridSystemData.GridMapArray[i].TargetGridPosition.Equals(targetGridPosition))
				{
					// Reuse existing map
					flowFieldFollower.ValueRW.GridIndex = i;
					flowFieldFollower.ValueRW.TargetPosition = fieldPathRequest.ValueRO.TargetPosition;
					flowFieldFollowerEnabled.ValueRW = true;
					alreadyCalculatedPath = true;
					break;
				}
			}

			if (alreadyCalculatedPath)
			{
				continue;
			}

			var gridIndex = gridSystemData.NextAvailableMapIndex;
			gridSystemData.NextAvailableMapIndex = (gridSystemData.NextAvailableMapIndex + 1) % FLOW_FIELD_MAP_COUNT;
			SystemAPI.SetComponent(state.SystemHandle, gridSystemData);

			flowFieldFollower.ValueRW.GridIndex = gridIndex;
			flowFieldFollower.ValueRW.TargetPosition = fieldPathRequest.ValueRO.TargetPosition;
			flowFieldFollowerEnabled.ValueRW = true;

			var gridNodeNativeArray = new NativeArray<RefRW<GridNode>>(gridSystemData.Width * gridSystemData.Height, Allocator.Temp);

			var initializeGridJob = new InitializeGridJob
			{
				GridIndex = gridIndex,
				TargetGridPosition = targetGridPosition
			};

			var intializeGridJobHandle = initializeGridJob.ScheduleParallel(state.Dependency);
			intializeGridJobHandle.Complete();

			for (var x = 0; x < gridSystemData.Width; x++)
			{
				for (var y = 0; y < gridSystemData.Height; y++)
				{
					var index = CalculateIndex(x, y, gridSystemData.Width);
					var gridNodeEntity = gridSystemData.GridMapArray[gridIndex].GridEntityArray[index];
					var gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);

					gridNodeNativeArray[index] = gridNode;
				}
			}


			var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
			var collisionWorld = physicsWorldSingleton.PhysicsWorld.CollisionWorld;

			var updateCostMapJob = new UpdateCostMapJob
			{
				Width = gridSystemData.Width,
				CellSize = gridSystemData.CellSize,
				HalfCellSize = gridSystemData.CellSize * 0.5f,
				CollisionWorld = collisionWorld,
				GridMap = gridSystemData.GridMapArray[gridIndex],
				GridCostArray = gridSystemData.GridCostArray,
				GridNodeLookup = GridNodeLookup
			};

			var updateCostMapJobHandle = updateCostMapJob.ScheduleParallel(gridSystemData.Width * gridSystemData.Height, 50, state.Dependency);
			updateCostMapJobHandle.Complete();

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

					var newMaxCost = currentGridNode.ValueRW.BestCost + neighborGridNode.ValueRO.Cost;

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

			var gridMap = gridSystemData.GridMapArray[gridIndex];
			gridMap.TargetGridPosition = targetGridPosition;
			gridMap.IsValid = true;
			gridSystemData.GridMapArray[gridIndex] = gridMap;
			SystemAPI.SetComponent(state.SystemHandle, gridSystemData);
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
		gridSystemData.ValueRW.GridCostArray.Dispose();
		gridSystemData.ValueRW.TotalGridMapEntityArray.Dispose();
	}
}

[BurstCompile]
public partial struct InitializeGridJob : IJobEntity
{
	public int GridIndex;
	public int2 TargetGridPosition;


	public void Execute(ref GridSystem.GridNode gridNode)
	{
		if (gridNode.GridIndex != GridIndex)
		{
			return;
		}

		gridNode.Vector = new float2(0, 1);
		if (gridNode.X == TargetGridPosition.x && gridNode.Y == TargetGridPosition.y)
		{
			// target destination

			gridNode.Cost = 0;
			gridNode.BestCost = 0;
		}
		else
		{
			gridNode.Cost = 1;
			gridNode.BestCost = int.MaxValue;
		}
	}
}

[BurstCompile]
public partial struct UpdateCostMapJob : IJobFor
{
	[NativeDisableParallelForRestriction] public ComponentLookup<GridSystem.GridNode> GridNodeLookup;
	[ReadOnly] public GridSystem.GridMap GridMap;
	[ReadOnly] public int Width;
	[ReadOnly] public float CellSize;
	[ReadOnly] public float HalfCellSize;

	[ReadOnly] public CollisionWorld CollisionWorld;

	[NativeDisableParallelForRestriction] public NativeArray<byte> GridCostArray;

	public void Execute(int index)
	{
		var distanceHitList = new NativeList<DistanceHit>(Allocator.TempJob);
		var gridPosition = GridSystem.GetGridPositionFromIndex(index, Width);
		// for walls
		if (CollisionWorld.OverlapSphere(GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, CellSize),
										 HalfCellSize,
										 ref distanceHitList,
										 GameConfig.PathfindingWallCollisionFilter))
		{
			var gridNode = GridNodeLookup[GridMap.GridEntityArray[index]];
			gridNode.Cost = GridSystem.WALL_COST;
			GridNodeLookup[GridMap.GridEntityArray[index]] = gridNode;

			GridCostArray[index] = GridSystem.WALL_COST;
		}

		// for heavy
		if (CollisionWorld.OverlapSphere(GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, CellSize),
										 HalfCellSize,
										 ref distanceHitList,
										 GameConfig.PathfindingHeavyCollisionFilter))
		{
			var gridNode = GridNodeLookup[GridMap.GridEntityArray[index]];
			gridNode.Cost = GridSystem.HEAVY_COST;
			GridNodeLookup[GridMap.GridEntityArray[index]] = gridNode;
			GridCostArray[index] = GridSystem.HEAVY_COST;
		}

		distanceHitList.Dispose();
	}
}
