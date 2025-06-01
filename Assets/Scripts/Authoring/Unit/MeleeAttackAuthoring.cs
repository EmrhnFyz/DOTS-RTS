using Unity.Entities;
using UnityEngine;

public class MeleeAttackAuthoring : MonoBehaviour
{
	public float cooldown;
	public float damageAmount;
	public float colliderSize;
	public float attackDistance;

	public class Baker : Baker<MeleeAttackAuthoring>
	{
		public override void Bake(MeleeAttackAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new MeleeAttack
			                     {
				                     Cooldown = authoring.cooldown,
				                     DamageAmount = authoring.damageAmount,
				                     ColliderSize = authoring.colliderSize,
				                     AttackDistance = authoring.attackDistance
			                     });
		}
	}
}

public struct MeleeAttack : IComponentData
{
	public float Timer;
	public float Cooldown;
	public float DamageAmount;
	public float ColliderSize;
	public float AttackDistance;

	public bool OnAttacked;
}