using UnityEngine;

public class GameConfig : MonoBehaviour
{
	public const int UNIT_LAYER = 6; // Layer for units
	public static GameConfig Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}
}