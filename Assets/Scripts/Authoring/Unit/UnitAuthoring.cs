using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
	public Factions faction;

	public class Baker : Baker<UnitAuthoring>
	{
		public override void Bake(UnitAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Unit
			                     {
				                     Faction = authoring.faction
			                     });
		}
	}
}

public struct Unit : IComponentData
{
	public Factions Faction;
}