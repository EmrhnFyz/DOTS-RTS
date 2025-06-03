using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
internal partial struct ResetEventsSystem : ISystem
{
	private NativeArray<JobHandle> _jobHandleNativeArray;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		_jobHandleNativeArray = new NativeArray<JobHandle>(4, Allocator.Persistent);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		_jobHandleNativeArray[0] = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
		_jobHandleNativeArray[1] = new ResetHealthEventsJob().ScheduleParallel(state.Dependency);
		_jobHandleNativeArray[2] = new ResetShootEventsJob().ScheduleParallel(state.Dependency);
		_jobHandleNativeArray[3] = new ResetMeleeAttackEventsJob().ScheduleParallel(state.Dependency);

		state.Dependency = JobHandle.CombineDependencies(_jobHandleNativeArray);
	}
}

[BurstCompile]
[WithPresent(typeof(Selected))]
public partial struct ResetSelectedEventsJob : IJobEntity
{
	public void Execute(ref Selected selected)
	{
		selected.OnSelected = false;
		selected.OnDeselected = false;
	}
}

[BurstCompile]
public partial struct ResetHealthEventsJob : IJobEntity
{
	public void Execute(ref Health health)
	{
		health.OnHealthChanged = false;
		health.OnDeath = false;
	}
}

[BurstCompile]
public partial struct ResetShootEventsJob : IJobEntity
{
	public void Execute(ref Shoot shoot)
	{
		shoot.OnShoot.IsTriggered = false;
	}
}


[BurstCompile]
public partial struct ResetMeleeAttackEventsJob : IJobEntity
{
	public void Execute(ref MeleeAttack meleeAttack)
	{
		meleeAttack.OnAttacked = false;
	}
}