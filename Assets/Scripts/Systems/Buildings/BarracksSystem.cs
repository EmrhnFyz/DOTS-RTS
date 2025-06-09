using Unity.Entities;
using Unity.Transforms;

internal partial struct BarracksSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EntitiesReferences>();
		state.RequireForUpdate<GameSceneTag>();
	}

	public void OnUpdate(ref SystemState state)
	{
		var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

		foreach (var (localTransform, barracks, spawnUnitTypeBuffers) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Barracks>, DynamicBuffer<SpawnUnitTypeBuffer>>())
		{
			if (spawnUnitTypeBuffers.IsEmpty)
			{
				continue;
			}

			if (barracks.ValueRO.UnitTypeToSpawn != spawnUnitTypeBuffers[0].UnitType)
			{
				barracks.ValueRW.UnitTypeToSpawn = spawnUnitTypeBuffers[0].UnitType;
				var unitToSpawnSO = GameConfig.Instance.unitTypeListSO.GetUnitTypeSO(barracks.ValueRW.UnitTypeToSpawn);
				barracks.ValueRW.SpawnTime = unitToSpawnSO.spawnTime;
			}

			barracks.ValueRW.Progress += SystemAPI.Time.DeltaTime;
			if (barracks.ValueRO.Progress < barracks.ValueRW.SpawnTime)
			{
				continue;
			}


			barracks.ValueRW.Progress = 0f;
			var unitTypeToSpawn = spawnUnitTypeBuffers[0].UnitType;
			var unitTypeSO = GameConfig.Instance.unitTypeListSO.GetUnitTypeSO(unitTypeToSpawn);

			spawnUnitTypeBuffers.RemoveAt(0); // Remove the first element after spawning

			var spawnedUnit = state.EntityManager.Instantiate(unitTypeSO.GetPrefabEntity(entitiesReferences));
			SystemAPI.SetComponent(spawnedUnit, LocalTransform.FromPosition(localTransform.ValueRO.Position));

			SystemAPI.SetComponent(spawnedUnit, new MoveOverride
			                                    {
				                                    TargetPosition = localTransform.ValueRO.Position + barracks.ValueRO.rallyPositionOffset
			                                    });

			SystemAPI.SetComponentEnabled<MoveOverride>(spawnedUnit, true);
		}
	}
}