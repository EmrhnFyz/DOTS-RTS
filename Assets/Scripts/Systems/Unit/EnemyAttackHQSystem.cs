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

		foreach (var (attackHq, target, unitMover) in SystemAPI.Query<RefRO<EnemyAttackHQ>, RefRO<Target>, RefRW<UnitMover>>().WithDisabled<MoveOverride>())
		{
			if (target.ValueRO.TargetEntity != Entity.Null)
			{
				continue;
			}

			unitMover.ValueRW.TargetPosition = hqPosition;
		}
	}
}