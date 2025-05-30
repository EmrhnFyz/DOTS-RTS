using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
internal partial struct ResetTargetSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var target in SystemAPI.Query<RefRW<Target>>())
		{
			if (target.ValueRO.TargetEntity == Entity.Null)
			{
				continue;
			}

			if (!SystemAPI.Exists(target.ValueRO.TargetEntity) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.TargetEntity))
			{
				target.ValueRW.TargetEntity = Entity.Null;
			}
		}

		foreach (var targetOverride in SystemAPI.Query<RefRW<TargetOverride>>())
		{
			if (targetOverride.ValueRO.TargetEntity == Entity.Null)
			{
				continue;
			}

			if (!SystemAPI.Exists(targetOverride.ValueRO.TargetEntity) || !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRO.TargetEntity))
			{
				targetOverride.ValueRW.TargetEntity = Entity.Null;
			}
		}
	}
}