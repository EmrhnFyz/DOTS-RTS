using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
internal partial struct ResetEventsSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		new ResetSelectedEventsJob().ScheduleParallel();
		new ResetHealthEventsJob().ScheduleParallel();
		new ResetShootEventsJob().ScheduleParallel();
		new ResetMeleeAttackEventsJob().ScheduleParallel();
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