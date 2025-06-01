using Unity.Entities;
using UnityEngine;

public class FactionAuthoring : MonoBehaviour
{
	public Factions factionType;

	public class Baker : Baker<FactionAuthoring>
	{
		public override void Bake(FactionAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Faction
			                     {
				                     FactionType = authoring.factionType
			                     });
		}
	}
}

public struct Faction : IComponentData
{
	public Factions FactionType;
}