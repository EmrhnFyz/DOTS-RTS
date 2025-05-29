using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
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
	}
}