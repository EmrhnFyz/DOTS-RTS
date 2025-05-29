using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
internal partial struct RestTargetSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var target in SystemAPI.Query<RefRW<Target>>())
		{
			if (!SystemAPI.Exists(target.ValueRO.TargetEntity))
			{
				target.ValueRW.TargetEntity = Entity.Null;
			}
		}
	}
}