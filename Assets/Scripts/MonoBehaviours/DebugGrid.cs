using UnityEngine;

public class DebugGrid : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	private int x;
	private int y;

	public void Setup(int x, int y, float gridCellSize)
	{
		this.x = x;
		this.y = y;
		transform.position = GridSystem.GetWorldPosition(x, y, gridCellSize);
		spriteRenderer.transform.localScale = new Vector3(gridCellSize, gridCellSize, gridCellSize);
		spriteRenderer.transform.localPosition = new Vector3(gridCellSize / 2, 0.1f, gridCellSize / 2);
		gameObject.name = $"DebugGrid ({x}, {y})";
	}

	public void SetColor(Color color)
	{
		spriteRenderer.color = color;
	}

	public void SetSprite(Sprite sprite)
	{
		spriteRenderer.sprite = sprite;
	}

	public void SetSpriteRotation(Quaternion rotation)
	{
		spriteRenderer.transform.rotation = rotation;
		spriteRenderer.transform.rotation *= Quaternion.Euler(90, 0, 0);
	}
}