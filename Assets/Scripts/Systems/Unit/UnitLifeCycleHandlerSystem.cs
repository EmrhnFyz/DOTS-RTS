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
		foreach (var (health, entity) in SystemAPI.Query<RefRW<Health>>().WithEntityAccess())
		{
			if (health.ValueRO.CurrentHealth <= 0)
			{
				health.ValueRW.OnDeath = true;
				entityCommandBuffer.DestroyEntity(entity);
			}
		}
	}
}