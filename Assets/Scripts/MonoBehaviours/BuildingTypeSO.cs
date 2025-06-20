using Unity.Entities;
using UnityEngine;

public enum BuildingType
{
	None,
	ZombieSpawner,
	Turret,
	Barracks,
	HQ,
	GoldHarvester,
	IronHarvester,
	OilHarvester
}


[CreateAssetMenu(fileName = "BuildingTypeSO", menuName = "ScriptableObjects/Buildings/BuildingTypeSO")]
public class BuildingTypeSO : ScriptableObject
{
	public string NameString;
	public BuildingType buildingType;
	public BoxCollider buildingCollider;

	public bool isPlaceable = true;

	public Sprite iconSprite;

	public Transform visualPrefab;

	public ResourceAmount[] cost;

	public float constructionTimerMax;
	public float constructionYOffset;

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
			case BuildingType.GoldHarvester:
				return references.GoldHarvesterPrefabEntity;
			case BuildingType.IronHarvester:
				return references.IronHarvesterPrefabEntity;
			case BuildingType.OilHarvester:
				return references.OilHarvesterPrefabEntity;
		}
	}

	public Entity GetConstructionVisualPrefabEntity(EntitiesReferences references)
	{
		switch (buildingType)
		{
			default:
			case BuildingType.None:
			case BuildingType.Barracks:
				return references.BarracksVisualPrefabEntity;
			case BuildingType.Turret:
				return references.TurretVisualPrefabEntity;
			case BuildingType.GoldHarvester:
				return references.GoldHarvesterVisualPrefabEntity;
			case BuildingType.IronHarvester:
				return references.IronHarvesterVisualPrefabEntity;
			case BuildingType.OilHarvester:
				return references.OilHarvesterVisualPrefabEntity;
		}
	}
}