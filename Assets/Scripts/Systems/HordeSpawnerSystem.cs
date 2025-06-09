using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

internal partial struct HordeSpawnerSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EntitiesReferences>();
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var endSimulationEntityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		var entityReferences = SystemAPI.GetSingleton<EntitiesReferences>();
		foreach (var (horde, localTransform) in SystemAPI.Query<RefRW<Horde>, RefRO<LocalTransform>>())
		{
			horde.ValueRW.StartTimer -= SystemAPI.Time.DeltaTime;
			if (horde.ValueRW.StartTimer > 0)
			{
				continue;
			}

			if (horde.ValueRO.ZombieAmountToSpawn <= 0)
			{
				continue;
			}

			horde.ValueRW.SpawnTimer -= SystemAPI.Time.DeltaTime;
			if (horde.ValueRO.SpawnTimer <= 0)
			{
				horde.ValueRW.SpawnTimer = horde.ValueRO.SpawnCooldown;
				var zombieEntity = endSimulationEntityCommandBuffer.Instantiate(entityReferences.ZombiePrefabEntity);

				var random = horde.ValueRO.Random;
				var spawnPosition = localTransform.ValueRO.Position;

				spawnPosition.x += random.NextFloat(-horde.ValueRO.SpawnAreaWidth, horde.ValueRO.SpawnAreaWidth);
				spawnPosition.z += random.NextFloat(-horde.ValueRO.SpawnAreaHeight, horde.ValueRO.SpawnAreaHeight);
				horde.ValueRW.Random = random;

				endSimulationEntityCommandBuffer.SetComponent(zombieEntity, LocalTransform.FromPosition(spawnPosition));
				endSimulationEntityCommandBuffer.AddComponent<EnemyAttackHQ>(zombieEntity);

				horde.ValueRW.ZombieAmountToSpawn--;
			}
		}
	}

	[BurstCompile]
	public void OnDestroy(ref SystemState state)
	{
	}
}