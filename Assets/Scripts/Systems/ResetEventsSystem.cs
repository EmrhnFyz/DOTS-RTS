using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
internal partial struct ResetEventsSystem : ISystem
{
	private NativeArray<JobHandle> _jobHandleNativeArray;

	private NativeList<Entity> _onDeathEntityList;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<HQ>();
		state.RequireForUpdate<GameSceneTag>();

		_jobHandleNativeArray = new NativeArray<JobHandle>(3, Allocator.Persistent);
		_onDeathEntityList = new NativeList<Entity>(Allocator.Persistent);
	}

	public void OnUpdate(ref SystemState state)
	{
		if (SystemAPI.HasSingleton<HQ>())
		{
			var hqHealth = SystemAPI.GetComponent<Health>(SystemAPI.GetSingletonEntity<HQ>());

			if (hqHealth.OnDeath)
			{
				DOTSEventManager.Instance.TriggerOnHQDeath();
			}
		}

		_jobHandleNativeArray[0] = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
		_jobHandleNativeArray[1] = new ResetShootEventsJob().ScheduleParallel(state.Dependency);
		_jobHandleNativeArray[2] = new ResetMeleeAttackEventsJob().ScheduleParallel(state.Dependency);

		_onDeathEntityList.Clear();
		new ResetHealthEventsJob
		{
			OnDeathEntityList = _onDeathEntityList.AsParallelWriter()
		}.ScheduleParallel(state.Dependency).Complete();
		DOTSEventManager.Instance.TriggerOnDeath(_onDeathEntityList);

		state.Dependency = JobHandle.CombineDependencies(_jobHandleNativeArray);
	}

	[BurstCompile]
	public void OnDestroy(ref SystemState state)
	{
		_jobHandleNativeArray.Dispose();
		_onDeathEntityList.Dispose();
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
	public NativeList<Entity>.ParallelWriter OnDeathEntityList;

	public void Execute(ref Health health, Entity entity)
	{
		if (health.OnDeath)
		{
			OnDeathEntityList.AddNoResize(entity);
		}

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