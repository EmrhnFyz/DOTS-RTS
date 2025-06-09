using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
internal partial struct ResetTargetSystem : ISystem
{
	private ComponentLookup<LocalTransform> _localTransformLookup;
	private EntityStorageInfoLookup _entityStorageInfoLookup;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<GameSceneTag>();

		_localTransformLookup = state.GetComponentLookup<LocalTransform>(true);

		_entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		_localTransformLookup.Update(ref state);
		_entityStorageInfoLookup.Update(ref state);

		var resetTargetJob = new ResetTargetJob
		                     {
			                     LocalTransformLookup = _localTransformLookup,
			                     EntityStorageInfoLookup = _entityStorageInfoLookup
		                     };

		resetTargetJob.ScheduleParallel();

		var resetTargetOverrideJob = new ResetTargetOverrideJob
		                             {
			                             LocalTransformLookup = _localTransformLookup,
			                             EntityStorageInfoLookup = _entityStorageInfoLookup
		                             };
		resetTargetOverrideJob.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct ResetTargetJob : IJobEntity
{
	[ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
	[ReadOnly] public EntityStorageInfoLookup EntityStorageInfoLookup;

	public void Execute(ref Target target)
	{
		if (target.TargetEntity == Entity.Null)
		{
			return;
		}

		if (!EntityStorageInfoLookup.Exists(target.TargetEntity) || !LocalTransformLookup.HasComponent(target.TargetEntity))
		{
			target.TargetEntity = Entity.Null;
		}
	}
}

[BurstCompile]
public partial struct ResetTargetOverrideJob : IJobEntity
{
	[ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
	[ReadOnly] public EntityStorageInfoLookup EntityStorageInfoLookup;

	public void Execute(ref TargetOverride targetOverride)
	{
		if (targetOverride.TargetEntity == Entity.Null)
		{
			return;
		}

		if (!EntityStorageInfoLookup.Exists(targetOverride.TargetEntity) || !LocalTransformLookup.HasComponent(targetOverride.TargetEntity))
		{
			targetOverride.TargetEntity = Entity.Null;
		}
	}
}