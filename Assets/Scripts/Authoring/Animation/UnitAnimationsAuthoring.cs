using Unity.Entities;
using UnityEngine;

public class UnitAnimationsAuthoring : MonoBehaviour
{
	public AnimationType idleAnimationType;
	public AnimationType walkAnimationType;
	public AnimationType shootAnimationType;
	public AnimationType aimAnimationType;
	public AnimationType meleeAttackAnimationType;

	public class Baker : Baker<UnitAnimationsAuthoring>
	{
		public override void Bake(UnitAnimationsAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new UnitAnimations
			                     {
				                     IdleAnimationType = authoring.idleAnimationType,
				                     WalkAnimationType = authoring.walkAnimationType,
				                     ShootAnimationType = authoring.shootAnimationType,
				                     AimAnimationType = authoring.aimAnimationType,
				                     MeleeAttackAnimationType = authoring.meleeAttackAnimationType
			                     });
		}
	}
}

public struct UnitAnimations : IComponentData
{
	public AnimationType IdleAnimationType;
	public AnimationType WalkAnimationType;
	public AnimationType ShootAnimationType;
	public AnimationType AimAnimationType;
	public AnimationType MeleeAttackAnimationType;
}