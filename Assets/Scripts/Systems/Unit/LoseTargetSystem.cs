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
		foreach (var (localTransform, loseTarget, target) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<LoseTarget>, RefRW<Target>>())
		{
			if (target.ValueRO.TargetEntity == Entity.Null)
			{
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