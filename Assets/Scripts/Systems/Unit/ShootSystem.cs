using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

internal partial struct ShootSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EntitiesReferences>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
		foreach (var (localTransform, shoot, target) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Shoot>, RefRO<Target>>())
		{
			if (target.ValueRO.targetEntity == Entity.Null)
			{
				continue;
			}

			shoot.ValueRW.timer -= SystemAPI.Time.DeltaTime;

			if (shoot.ValueRO.timer > 0f)
			{
				continue;
			}

			shoot.ValueRW.timer = shoot.ValueRO.cooldown;

			var bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);
			SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position + new float3(0, 1, 0)));
			var bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
			bullet.ValueRW.damageAmount = shoot.ValueRO.damageAmount;

			if (SystemAPI.HasComponent<LocalTransform>(target.ValueRO.targetEntity))
			{
				var targetPosition = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity).Position;
				bullet.ValueRW.direction = math.normalize(targetPosition - localTransform.ValueRO.Position);
			}
			else
			{
				// Default forward direction if target doesn't exist
				bullet.ValueRW.direction = math.forward();
			}
		}
	}
}