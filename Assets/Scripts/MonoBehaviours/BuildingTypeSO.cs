using UnityEngine;

public enum BuildingType
{
	None,
	ZombieSpawner,
	Turret,
	Barracks
}


[CreateAssetMenu(fileName = "BuildingTypeSO", menuName = "ScriptableObjects/Buildings/BuildingTypeSO")]
public class BuildingTypeSO : ScriptableObject
{
	public BuildingType buildingType;
}