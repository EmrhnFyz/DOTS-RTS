using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

internal partial struct MoveOverrideSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<GameSceneTag>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (localTransform, moveOverride, moveOverrideEnabled, unitMover) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveOverride>, EnabledRefRW<MoveOverride>, RefRW<UnitMover>>())
		{
			if (math.distancesq(localTransform.ValueRO.Position, moveOverride.ValueRO.TargetPosition) > GameConfig.REACH_TARGET_DISTANCE_SQ)
			{
				// move closer

				unitMover.ValueRW.TargetPosition = moveOverride.ValueRO.TargetPosition;
			}
			else
			{
				// reached move override target position

				moveOverrideEnabled.ValueRW = false;
			}
		}
	}
}