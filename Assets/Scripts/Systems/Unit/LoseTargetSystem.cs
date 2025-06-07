using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

internal partial struct LoseTargetSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (localTransform, loseTarget, targetOverride, target)
		         in SystemAPI.Query<RefRO<LocalTransform>, RefRO<LoseTarget>, RefRO<TargetOverride>, RefRW<Target>>())
		{
			if (target.ValueRO.TargetEntity == Entity.Null)
			{
				continue;
			}

			if (targetOverride.ValueRO.TargetEntity != Entity.Null)
			{
				// If a target override is set, we do not lose the target
				continue;
			}

			var targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.TargetEntity);
			var distanceToTarget = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);

			if (distanceToTarget > loseTarget.ValueRO.LoseDistance)
			{
				target.ValueRW.TargetEntity = Entity.Null;
			}
		}
	}
}