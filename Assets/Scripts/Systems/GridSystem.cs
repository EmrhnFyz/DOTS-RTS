#define GRID_DEBUG
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct GridSystem : ISystem
{
	public struct GridSystemData : IComponentData
	{
		public int Width;
		public int Height;
		public float CellSize;
		public GridMap GridMap;
	}

	public struct GridMap
	{
		public NativeArray<Entity> GridEntityArray;
	}

	public struct GridNode : IComponentData
	{
		public int X;
		public int Y;
		public byte Data;
	}

	public static int CalculateIndex(int x, int y, int width) => x + y * width;
	public static float3 GetWorldPosition(int x, int y, float cellSize) => new(x * cellSize, 0, y * cellSize);

	public static bool IsValidGridPosition(int2 gridPosition, int width, int height) => gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;

	public static int2 GetGridPosition(float3 worldPosition, float cellSize)
	{
		var x = (int)math.floor(worldPosition.x / cellSize);
		var y = (int)math.floor(worldPosition.z / cellSize);
		return new int2(x, y);
	}

#if !GRID_DEBUG
	[BurstCompile]
#endif
	public void OnCreate(ref SystemState state)
	{
		var width = 20;
		var height = 20;
		var gridCellSize = 2.0f;
		var totalCells = width * height;
		var gridNodeEntityPrefab = state.EntityManager.CreateEntity();
		state.EntityManager.AddComponent<GridNode>(gridNodeEntityPrefab);

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
					               X = x,
					               Y = y
				               };
#if GRID_DEBUG
				state.EntityManager.SetName(gridMap.GridEntityArray[index], $"GridNode_{x}_{y}");
#endif
				SystemAPI.SetComponent(gridMap.GridEntityArray[index], gridNode);
			}
		}

		state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
		state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
		                                                         {
			                                                         Width = width,
			                                                         Height = height,
			                                                         CellSize = gridCellSize,
			                                                         GridMap = gridMap
		                                                         });
	}
#if !GRID_DEBUG
	[BurstCompile]
#endif
	public void OnUpdate(ref SystemState state)
	{
		var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

		///~TEST CODE START
		if (Input.GetMouseButtonDown(0))
		{
			var mouseWorldPosition = MouseWorldPosition.Instance.GetMousePosition();
			var mouseGridPosition = GetGridPosition(mouseWorldPosition, gridSystemData.CellSize);
			if (IsValidGridPosition(mouseGridPosition, gridSystemData.Width, gridSystemData.Height))
			{
				var index = CalculateIndex(mouseGridPosition.x, mouseGridPosition.y, gridSystemData.Width);

				var gridNodeEntity = gridSystemData.GridMap.GridEntityArray[index];
				var gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);
				gridNode.ValueRW.Data = 1;
			}
		}
		///~TEST CODE END 
#if GRID_DEBUG
		GridSystemDebug.Instance?.InitializeGrid(gridSystemData);
		GridSystemDebug.Instance?.UpdateGird(gridSystemData);
#endif
	}

	[BurstCompile]
	public void OnDestroy(ref SystemState state)
	{
		var gridSystemData = SystemAPI.GetComponentRW<GridSystemData>(state.SystemHandle);
		gridSystemData.ValueRW.GridMap.GridEntityArray.Dispose();
	}
}