using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

internal partial struct ZombieSpawnerSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
		state.RequireForUpdate<EntitiesReferences>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
		var entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		foreach (var (localTransform, zombieSpawner) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
		{
			zombieSpawner.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

			if (zombieSpawner.ValueRO.Timer > 0f)
			{
				continue;
			}

			zombieSpawner.ValueRW.Timer = zombieSpawner.ValueRO.Cooldown;

			var zombieEntity = state.EntityManager.Instantiate(entitiesReferences.ZombiePrefabEntity);
			SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

			entityCommandBuffer.AddComponent(zombieEntity, new RandomWalking
			                                               {
				                                               OriginPosition = localTransform.ValueRO.Position,
				                                               TargetPosition = localTransform.ValueRO.Position,
				                                               DistanceMin = zombieSpawner.ValueRO.RandomWalkingDistanceMin,
				                                               DistanceMax = zombieSpawner.ValueRO.RandomWalkingDistanceMax,
				                                               Rng = new Random((uint)zombieEntity.Index)
			                                               });
		}
	}
}