using Unity.Entities;
using UnityEngine;

public class MeleeAttackAuthoring : MonoBehaviour
{
	public float cooldown;

	public class Baker : Baker<MeleeAttackAuthoring>
	{
		public override void Bake(MeleeAttackAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new MeleeAttack
			                     {
				                     Cooldown = authoring.cooldown
			                     });
		}
	}
}

public struct MeleeAttack : IComponentData
{
	public float Timer;
	public float Cooldown;
}