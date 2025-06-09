using Unity.Entities;
using UnityEngine;

public enum UnitType
{
	None,
	Scout,
	Soldier,
	Zombie
}

[CreateAssetMenu(fileName = "UnitTypeSO", menuName = "ScriptableObjects/Units/UnitTypeSO")]
public class UnitTypeSO : ScriptableObject
{
	public UnitType unitType;
	public float spawnTime;
	public ResourceAmount[] cost;

	public Entity GetPrefabEntity(EntitiesReferences entitiesReference)
	{
		switch (unitType)
		{
			default:
			case UnitType.None:
			case UnitType.Soldier:
				return entitiesReference.SoldierPrefabEntity;
			case UnitType.Scout:
				return entitiesReference.ScoutPrefabEntity;
			case UnitType.Zombie:
				return entitiesReference.ZombiePrefabEntity;
		}
	}
}