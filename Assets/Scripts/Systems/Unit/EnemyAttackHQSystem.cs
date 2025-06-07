using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

internal partial struct EnemyAttackHQSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<HQ>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var hqEntity = SystemAPI.GetSingletonEntity<HQ>();

		var hqPosition = SystemAPI.GetComponent<LocalTransform>(hqEntity).Position;

		foreach (var (attackHq, target, targetPositionPathQueued, targetPositionPathQueuedEnabled)
		         in SystemAPI.Query<RefRO<EnemyAttackHQ>, RefRO<Target>, RefRW<TargetPositionPathQueued>, EnabledRefRW<TargetPositionPathQueued>>().WithDisabled<MoveOverride>().WithPresent<TargetPositionPathQueued>())
		{
			if (target.ValueRO.TargetEntity != Entity.Null)
			{
				continue;
			}

			targetPositionPathQueued.ValueRW.TargetPosition = hqPosition;
			targetPositionPathQueuedEnabled.ValueRW = true;
		}
	}
}