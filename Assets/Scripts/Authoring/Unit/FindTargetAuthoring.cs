using Unity.Entities;
using UnityEngine;

public class FindTargetAuthoring : MonoBehaviour
{
	public float range;
	public float cooldown;
	public Factions targetFaction;

	public class Baker : Baker<FindTargetAuthoring>
	{
		public override void Bake(FindTargetAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FindTarget
			                     {
				                     range = authoring.range,
				                     cooldown = authoring.cooldown,
				                     targetFaction = authoring.targetFaction
			                     });
		}
	}
}

public struct FindTarget : IComponentData
{
	public float range;
	public float timer;
	public float cooldown;
	public Factions targetFaction;
}