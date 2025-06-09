using Unity.Burst;
using Unity.Entities;

internal partial struct ShootLightDestroySystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
		state.RequireForUpdate<GameSceneTag>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		foreach (var (shootLight, entity) in SystemAPI.Query<RefRW<ShootLight>>().WithEntityAccess())
		{
			shootLight.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

			if (shootLight.ValueRO.Timer <= 0f)
			{
				entityCommandBuffer.DestroyEntity(entity);
			}
		}
	}
}