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
				                     Range = authoring.range,
				                     Cooldown = authoring.cooldown,
				                     TargetFaction = authoring.targetFaction
			                     });
		}
	}
}

public struct FindTarget : IComponentData
{
	public float Range;
	public float Timer;
	public float Cooldown;
	public Factions TargetFaction;
}