using UnityEngine;

public class DebugGrid : MonoBehaviour
{
	[SerializeField] private SpriteRenderer gridImagePrefab;

	private int x;
	private int y;

	public void Setup(int x, int y, float gridCellSize)
	{
		this.x = x;
		this.y = y;
		transform.position = GridSystem.GetWorldPosition(x, y, gridCellSize);
		gridImagePrefab.transform.localScale = new Vector3(gridCellSize, gridCellSize, gridCellSize);
		gridImagePrefab.transform.localPosition = new Vector3(gridCellSize / 2, 0.1f, gridCellSize / 2);
		gameObject.name = $"DebugGrid ({x}, {y})";
	}

	public void SetColor(Color color)
	{
		gridImagePrefab.color = color;
	}
}