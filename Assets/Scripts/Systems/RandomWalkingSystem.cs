using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

internal partial struct RandomWalkingSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (randomWalking, targetPositionPathQueued, targetPositionPathQueuedEnabled, localTransform)
		         in SystemAPI.Query<RefRW<RandomWalking>, RefRW<TargetPositionPathQueued>, EnabledRefRW<TargetPositionPathQueued>, RefRO<LocalTransform>>().WithPresent<TargetPositionPathQueued>())
		{
			if (math.distancesq(localTransform.ValueRO.Position, randomWalking.ValueRO.TargetPosition) < GameConfig.REACH_TARGET_DISTANCE_SQ)
			{
				var rng = randomWalking.ValueRO.Rng;

				var randomDirection = math.normalize(new float3(rng.NextFloat(-1f, +1f), 0, rng.NextFloat(-1f, +1f)));

				randomWalking.ValueRW.TargetPosition = randomWalking.ValueRO.OriginPosition + randomDirection * rng.NextFloat(randomWalking.ValueRO.DistanceMin, randomWalking.ValueRO.DistanceMax);

				randomWalking.ValueRW.Rng = rng;
			}
			else
			{
				targetPositionPathQueued.ValueRW.TargetPosition = randomWalking.ValueRO.TargetPosition;
				targetPositionPathQueuedEnabled.ValueRW = true;
			}
		}
	}
}