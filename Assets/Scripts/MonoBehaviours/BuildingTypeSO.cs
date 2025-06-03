using Unity.Entities;
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
	public BoxCollider buildingCollider;

	public bool isPlaceable = true;

	public Sprite iconSprite;

	public Transform visualPrefab;

	public Entity GetBuildingPrefabEntity(EntitiesReferences references)
	{
		switch (buildingType)
		{
			default:
			case BuildingType.None:
			case BuildingType.Barracks:
				return references.BarracksPrefabEntity;
			case BuildingType.Turret:
				return references.TurretPrefabEntity;
		}
	}
}