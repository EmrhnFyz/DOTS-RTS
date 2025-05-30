using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(ShootSystem))]
internal partial struct AnimationStateSystem : ISystem
{
	private ComponentLookup<ActiveAnimation> _activeAnimationLookup;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		_activeAnimationLookup = state.GetComponentLookup<ActiveAnimation>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		_activeAnimationLookup.Update(ref state);

		var idleWalkingAnimationStateJob = new IdleWalkingAnimationStateJob
		                                   {
			                                   ActiveAnimationLookup = _activeAnimationLookup
		                                   };
		idleWalkingAnimationStateJob.ScheduleParallel();

		_activeAnimationLookup.Update(ref state);
		var aimShootAnimationStateJob = new AimShootAnimationStateJob
		                                {
			                                ActiveAnimationLookup = _activeAnimationLookup
		                                };
		aimShootAnimationStateJob.ScheduleParallel();

		_activeAnimationLookup.Update(ref state);
		var meleeAttackAnimationStateJob = new MeleeAttackAnimationStateJob
		                                   {
			                                   ActiveAnimationLookup = _activeAnimationLookup
		                                   };
		meleeAttackAnimationStateJob.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct IdleWalkingAnimationStateJob : IJobEntity
{
	[NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> ActiveAnimationLookup;

	public void Execute(in AnimatedMesh animatedMesh, in UnitMover unitMover, in UnitAnimations unitAnimations)
	{
		var activeAnimation = ActiveAnimationLookup.GetRefRW(animatedMesh.MeshEntity);

		if (unitMover.IsMoving)
		{
			activeAnimation.ValueRW.NextAnimationType = unitAnimations.WalkAnimationType;
		}
		else
		{
			activeAnimation.ValueRW.NextAnimationType = unitAnimations.IdleAnimationType;
		}
	}
}

[BurstCompile]
public partial struct AimShootAnimationStateJob : IJobEntity
{
	[NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> ActiveAnimationLookup;

	public void Execute(ref AnimatedMesh animatedMesh, in Shoot shoot, in UnitAnimations unitAnimations, in UnitMover unitMover, in Target target)
	{
		var activeAnimation = ActiveAnimationLookup.GetRefRW(animatedMesh.MeshEntity);

		if (!unitMover.IsMoving && target.TargetEntity != Entity.Null)
		{
			activeAnimation.ValueRW.NextAnimationType = unitAnimations.AimAnimationType;
		}

		if (shoot.OnShoot.IsTriggered)
		{
			activeAnimation.ValueRW.NextAnimationType = unitAnimations.ShootAnimationType;
		}
	}
}

[BurstCompile]
public partial struct MeleeAttackAnimationStateJob : IJobEntity
{
	[NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> ActiveAnimationLookup;

	public void Execute(in AnimatedMesh animatedMesh, in MeleeAttack meleeAttack, in UnitAnimations unitAnimations)
	{
		var activeAnimation = ActiveAnimationLookup.GetRefRW(animatedMesh.MeshEntity);

		if (meleeAttack.OnAttacked)
		{
			activeAnimation.ValueRW.NextAnimationType = unitAnimations.MeleeAttackAnimationType;
		}
	}
}