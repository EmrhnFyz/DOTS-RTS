using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
internal partial struct ResetEventsSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
		{
			selected.ValueRW.OnSelected = false;
			selected.ValueRW.OnDeselected = false;
		}

		foreach (var health in SystemAPI.Query<RefRW<Health>>())
		{
			health.ValueRW.OnHealthChanged = false;
		}

		foreach (var shoot in SystemAPI.Query<RefRW<Shoot>>())
		{
			shoot.ValueRW.OnShoot.IsTriggered = false;
		}
	}
}