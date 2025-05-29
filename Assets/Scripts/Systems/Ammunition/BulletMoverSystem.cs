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
			// Move in the set Direction
			var moveDistance = bullet.ValueRO.Speed * deltaTime;
			localTransform.ValueRW.Position += bullet.ValueRO.Direction * moveDistance;

			// Track distance for cleanup
			bullet.ValueRW.DistanceTraveled += moveDistance;

			// Destroy if max distance reached
			if (bullet.ValueRO.DistanceTraveled > bullet.ValueRO.MaxDistance)
			{
				entityCommandBuffer.DestroyEntity(entity);
			}
		}
	}
}