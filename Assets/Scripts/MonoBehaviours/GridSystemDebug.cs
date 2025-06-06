using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GridSystemDebug : MonoBehaviour
{
	public static GridSystemDebug Instance { get; private set; }
	[SerializeField] private Transform debugGridPrefab;

	[SerializeField] private Sprite arrowSprite;
	[SerializeField] private Sprite circleSprite;

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
				var gridIndex = math.clamp(data.NextAvailableMapIndex - 1, 0, GridSystem.FLOW_FIELD_MAP_COUNT);
				var gridNodeEntity = data.GridMapArray[gridIndex].GridEntityArray[index];
				var gridNode = entityManager.GetComponentData<GridSystem.GridNode>(gridNodeEntity);

				if (gridNode.Cost == 0)
				{
					debugGrid.SetSprite(circleSprite);
					debugGrid.SetColor(Color.green);
				}
				else
				{
					if (gridNode.Cost == GridSystem.WALL_COST)
					{
						debugGrid.SetSprite(circleSprite);
						debugGrid.SetColor(Color.black);
					}
					else
					{
						debugGrid.SetSprite(arrowSprite);
						debugGrid.SetSpriteRotation(Quaternion.LookRotation(new float3(gridNode.Vector.x, 0, gridNode.Vector.y), Vector3.up));
						debugGrid.SetColor(Color.white);
					}
				}
			}
		}
	}
}