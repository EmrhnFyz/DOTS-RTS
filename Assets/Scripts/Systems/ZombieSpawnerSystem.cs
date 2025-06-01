using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

internal partial struct ZombieSpawnerSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
		state.RequireForUpdate<EntitiesReferences>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
		var entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
		var collisionWorld = physicsWorldSingleton.CollisionWorld;
		var distanceHits = new NativeList<DistanceHit>(Allocator.Temp);

		// Iterate over all zombie spawners in the world
		foreach (var (localTransform, zombieSpawner) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
		{
			zombieSpawner.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

			if (zombieSpawner.ValueRO.Timer > 0f)
			{
				continue;
			}

			zombieSpawner.ValueRW.Timer = zombieSpawner.ValueRO.Cooldown;
			distanceHits.Clear();

			var nearbyZombieAmount = 0;

			// Check for nearby zombies using a physics overlap sphere
			if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, zombieSpawner.ValueRO.NearbyZombieDistanceThreshold, ref distanceHits, GameConfig.UnitSelectionCollisionFilter))
			{
				foreach (var hit in distanceHits)
				{
					// Skip if the entity no longer exists
					if (!SystemAPI.Exists(hit.Entity))
					{
						continue;
					}

					// Count only entities that are both Unit and Enemy (i.e., zombies)
					if (SystemAPI.HasComponent<Unit>(hit.Entity) && SystemAPI.HasComponent<Enemy>(hit.Entity))
					{
						nearbyZombieAmount++;
					}
				}
			}

			// If there are too many zombies nearby, skip spawning
			if (nearbyZombieAmount >= zombieSpawner.ValueRO.NearbyZombieTotalZombieCount)
			{
				continue;
			}

			// Instantiate a new zombie entity at the spawner's position
			var zombieEntity = state.EntityManager.Instantiate(entitiesReferences.ZombiePrefabEntity);
			SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

			// Add a RandomWalking component to the new zombie to enable random movement
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