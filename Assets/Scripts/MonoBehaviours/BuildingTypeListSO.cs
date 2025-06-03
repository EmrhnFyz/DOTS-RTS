using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingTypeSOList", menuName = "ScriptableObjects/Buildings/BuildingTypeSOList")]
public class BuildingTypeListSO : ScriptableObject
{
	public List<BuildingTypeSO> buildingTypeSOList;

	public BuildingTypeSO none;

	public BuildingTypeSO GetBuildingTypeSO(BuildingType buildingType)
	{
		foreach (var buildingTypeSO in buildingTypeSOList)
		{
			if (buildingTypeSO.buildingType == buildingType)
			{
				return buildingTypeSO;
			}
		}

		return null; // Return null if no matching BuildingTypeSO is found
	}
}