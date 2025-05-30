using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateAfter(typeof(ShootSystem))]
[UpdateBefore(typeof(ShootLightDestroySystem))]
internal partial struct ShootLightSpawnSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EntitiesReferences>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

		foreach (var (shoot, entity) in SystemAPI.Query<RefRO<Shoot>>().WithEntityAccess())
		{
			if (!shoot.ValueRO.OnShoot.IsTriggered)
			{
				continue;
			}

			var shootLightEntity = state.EntityManager.Instantiate(entitiesReferences.ShootLightPrefabEntity);
			SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shoot.ValueRO.OnShoot.ShootFromPosition));
		}
	}
}