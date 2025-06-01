using Unity.Entities;
using UnityEngine;

public class UnitTypeHolderAuthoring : MonoBehaviour
{
	public UnitType unitType;

	private class Baker : Baker<UnitTypeHolderAuthoring>
	{
		public override void Bake(UnitTypeHolderAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new UnitTypeHolder
			                     {
				                     UnitType = authoring.unitType
			                     });
		}
	}
}

public struct UnitTypeHolder : IComponentData
{
	public UnitType UnitType;
}