using Unity.Entities;
using UnityEngine;

public class GridSystemDebug : MonoBehaviour
{
	public static GridSystemDebug Instance { get; private set; }
	[SerializeField] private Transform debugGridPrefab;
	private bool _isInitialized;

	private DebugGrid[,] _debugGridMatrix;

	private void Awake()
	{
		Instance = this;
	}

	public void InitializeGrid(GridSystem.GridSystemData data)
	{
		if (_isInitialized)
		{
			return;
		}

		_isInitialized = true;
		_debugGridMatrix = new DebugGrid[data.Width, data.Height];
		for (var x = 0; x < data.Width; x++)
		{
			for (var y = 0; y < data.Height; y++)
			{
				var gridCell = Instantiate(debugGridPrefab, new Vector3(x, 0, y), Quaternion.identity);
				var debugGrid = gridCell.GetComponent<DebugGrid>();
				debugGrid.Setup(x, y, data.CellSize);
				_debugGridMatrix[x, y] = debugGrid;
			}
		}
	}

	public void UpdateGird(GridSystem.GridSystemData data)
	{
		for (var x = 0; x < data.Width; x++)
		{
			for (var y = 0; y < data.Height; y++)
			{
				var debugGrid = _debugGridMatrix[x, y];
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				var index = GridSystem.CalculateIndex(x, y, data.Width);
				var gridNodeEntity = data.GridMap.GridEntityArray[index];
				var gridNode = entityManager.GetComponentData<GridSystem.GridNode>(gridNodeEntity);

				debugGrid.SetColor(gridNode.Data == 0 ? Color.white : Color.blue);
			}
		}
	}
}