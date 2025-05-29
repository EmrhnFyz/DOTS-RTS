using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

internal partial struct BulletMoverSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		var deltaTime = SystemAPI.Time.DeltaTime;

		foreach (var (localTransform, bullet, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Bullet>>().WithEntityAccess())
		{
			// Move in the set direction
			var moveDistance = bullet.ValueRO.speed * deltaTime;
			localTransform.ValueRW.Position += bullet.ValueRO.direction * moveDistance;

			// Track distance for cleanup
			bullet.ValueRW.distanceTraveled += moveDistance;

			// Destroy if max distance reached
			if (bullet.ValueRO.distanceTraveled > bullet.ValueRO.maxDistance)
			{
				entityCommandBuffer.DestroyEntity(entity);
			}
		}
	}
}