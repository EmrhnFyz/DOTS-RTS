using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(ResetEventsSystem))]
internal partial struct UnitSelectionSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<GameSceneTag>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>())
		{
			if (selected.ValueRO.OnDeselected)
			{
				var selectionMark = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.SelectionMark);
				selectionMark.ValueRW.Scale = 0f;
			}

			if (selected.ValueRO.OnSelected)
			{
				var selectionMark = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.SelectionMark);
				selectionMark.ValueRW.Scale = selected.ValueRO.ShowScale;
			}
		}
	}
}