using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

internal partial struct ConstructionSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var endSimulationEntityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

		foreach (var (construction, localTransform, entity) in SystemAPI.Query<RefRW<Construction>, RefRO<LocalTransform>>().WithEntityAccess())
		{
			construction.ValueRW.ConstructionTimer += SystemAPI.Time.DeltaTime;
			var visualEntityLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(construction.ValueRO.VisualEntity);
			visualEntityLocalTransform.ValueRW.Position = math.lerp(construction.ValueRO.StartPosition, construction.ValueRO.EndPosition, construction.ValueRO.ConstructionTimer / construction.ValueRO.ConstructionTimeMax);

			if (construction.ValueRO.ConstructionTimer >= construction.ValueRO.ConstructionTimeMax)
			{
				var buildingEntity = endSimulationEntityCommandBuffer.Instantiate(construction.ValueRO.FinalPrefabEntity);
				endSimulationEntityCommandBuffer.SetComponent(buildingEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
				endSimulationEntityCommandBuffer.DestroyEntity(construction.ValueRO.VisualEntity);
				endSimulationEntityCommandBuffer.DestroyEntity(entity);
			}
		}
	}

	[BurstCompile]
	public void OnDestroy(ref SystemState state)
	{
	}
}