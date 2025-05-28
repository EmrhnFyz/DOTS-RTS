using Unity.Burst;
using Unity.Entities;

[UpdateBefore(typeof(ShootSystem))]
internal partial struct UnitLifeCycleHandlerSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		foreach (var (health, entity) in SystemAPI.Query<RefRO<Health>>().WithEntityAccess())
		{
			if (health.ValueRO.currentHealth <= 0)
			{
				entityCommandBuffer.DestroyEntity(entity);
			}
		}
	}
}