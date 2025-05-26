using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(ResetEventsSystem))]
internal partial struct UnitSelectionSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>())
		{
			if (selected.ValueRO.onDeselected)
			{
				var selectionMark = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.selectionMark);
				selectionMark.ValueRW.Scale = 0f;
			}

			if (selected.ValueRO.onSelected)
			{
				var selectionMark = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.selectionMark);
				selectionMark.ValueRW.Scale = selected.ValueRO.showScale;
			}
		}
	}
}