using Unity.Burst;
using Unity.Entities;

[UpdateAfter(typeof(ShootSystem))]
internal partial struct AnimationStateSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (animatedMesh, unitMover, unitAnimations) in SystemAPI.Query<RefRW<AnimatedMesh>, RefRO<UnitMover>, RefRO<UnitAnimations>>())
		{
			var activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.MeshEntity);

			if (unitMover.ValueRO.IsMoving)
			{
				activeAnimation.ValueRW.NextAnimationType = unitAnimations.ValueRO.WalkAnimationType;
			}
			else
			{
				activeAnimation.ValueRW.NextAnimationType = unitAnimations.ValueRO.IdleAnimationType;
			}
		}

		foreach (var (animatedMesh, shoot, unitAnimations, unitMover, target)
		         in SystemAPI.Query<RefRW<AnimatedMesh>, RefRO<Shoot>, RefRO<UnitAnimations>, RefRO<UnitMover>, RefRO<Target>>())
		{
			var activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.MeshEntity);

			if (!unitMover.ValueRO.IsMoving && target.ValueRO.TargetEntity != Entity.Null)
			{
				activeAnimation.ValueRW.NextAnimationType = unitAnimations.ValueRO.AimAnimationType;
			}

			if (shoot.ValueRO.OnShoot.IsTriggered)
			{
				activeAnimation.ValueRW.NextAnimationType = unitAnimations.ValueRO.ShootAnimationType;
			}
		}

		foreach (var (animatedMesh, meleeAttack, unitAnimations)
		         in SystemAPI.Query<RefRW<AnimatedMesh>, RefRO<MeleeAttack>, RefRO<UnitAnimations>>())
		{
			var activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.MeshEntity);

			if (meleeAttack.ValueRO.OnAttacked)
			{
				activeAnimation.ValueRW.NextAnimationType = unitAnimations.ValueRO.MeleeAttackAnimationType;
			}
		}
	}
}