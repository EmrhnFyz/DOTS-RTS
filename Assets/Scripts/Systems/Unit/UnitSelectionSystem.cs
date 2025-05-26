using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

internal partial struct UnitSelectionSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var selected in SystemAPI.Query<RefRO<Selected>>().WithDisabled<Selected>())
		{
			var selectionMarkTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.selectionMark);
			selectionMarkTransform.ValueRW.Scale = 0f;
		}

		foreach (var selected in SystemAPI.Query<RefRO<Selected>>())
		{
			var selectionMarkTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.selectionMark);
			selectionMarkTransform.ValueRW.Scale = selected.ValueRO.showScale;
		}
	}
}