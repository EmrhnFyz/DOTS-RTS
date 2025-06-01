using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitTypeSOList", menuName = "ScriptableObjects/Units/UnitTypeSOList")]
public class UnitTypeListSO : ScriptableObject
{
	public List<UnitTypeSO> unitTypeSOList;

	public UnitTypeSO GetUnitTypeSO(UnitType unitType)
	{
		foreach (var unitTypeSO in unitTypeSOList)
		{
			if (unitTypeSO.unitType == unitType)
			{
				return unitTypeSO;
			}
		}

		return null; // Return null if no matching UnitTypeSO is found
	}
}