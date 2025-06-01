using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

internal partial struct ShootSystem : ISystem
{
	private EntitiesReferences _entitiesReferences;

	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EntitiesReferences>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
		foreach (var (localTransform, shoot, target, findTarget, unitMover)
		         in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Shoot>, RefRO<Target>, RefRO<FindTarget>, RefRW<UnitMover>>().WithDisabled<MoveOverride>())
		{
			if (target.ValueRO.TargetEntity == Entity.Null)
			{
				continue;
			}

			var targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.TargetEntity);
			var distanceToTarget = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);

			if (distanceToTarget > findTarget.ValueRO.Range - GameConfig.TARGET_PROXIMITY_TRESHOLD)
			{
				unitMover.ValueRW.TargetPosition = targetLocalTransform.Position;
				continue;
			}

			unitMover.ValueRW.TargetPosition = localTransform.ValueRO.Position;
			var aimDirection = math.normalize(targetLocalTransform.Position - localTransform.ValueRO.Position);

			var targetRotation = quaternion.LookRotation(aimDirection, math.up());
			localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * unitMover.ValueRO.RotationSpeed);
		}

		foreach (var (rotator, target) in SystemAPI.Query<RefRO<TurretRotator>, RefRO<Target>>())
		{
			if (target.ValueRO.TargetEntity == Entity.Null)
			{
				continue;
			}

			// Safety checks
			if (!SystemAPI.HasComponent<LocalToWorld>(target.ValueRO.TargetEntity) ||
			    !SystemAPI.HasComponent<LocalToWorld>(rotator.ValueRO.TurretHeadEntity) ||
			    !SystemAPI.HasComponent<LocalTransform>(rotator.ValueRO.TurretHeadEntity))
			{
				continue;
			}

			// Get world positions
			var targetWorld = SystemAPI.GetComponent<LocalToWorld>(target.ValueRO.TargetEntity);
			var headWorld = SystemAPI.GetComponent<LocalToWorld>(rotator.ValueRO.TurretHeadEntity);
			var headLocal = SystemAPI.GetComponentRW<LocalTransform>(rotator.ValueRO.TurretHeadEntity);

			var toTarget = math.normalize(targetWorld.Position - headWorld.Position);

			// Flatten to Y axis to prevent turret tilting up/down
			toTarget.y = 0f;

			if (math.lengthsq(toTarget) < 0.0001f)
			{
				continue;
			}

			var targetRotation = quaternion.LookRotationSafe(toTarget, math.up());

			// Apply rotation smoothly
			headLocal.ValueRW.Rotation = math.slerp(headLocal.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * 5f);
		}

		foreach (var (localTransform, shoot, target, findTarget, entity)
		         in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Shoot>, RefRO<Target>, RefRO<FindTarget>>().WithEntityAccess())
		{
			if (target.ValueRO.TargetEntity == Entity.Null)
			{
				continue;
			}

			var targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.TargetEntity);
			if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > findTarget.ValueRO.Range)
			{
				continue;
			}

			if (SystemAPI.HasComponent<MoveOverride>(entity) && SystemAPI.IsComponentEnabled<MoveOverride>(entity))
			{
				continue;
			}

			shoot.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

			if (shoot.ValueRO.Timer > 0f)
			{
				continue;
			}

			shoot.ValueRW.Timer = shoot.ValueRO.Cooldown;


			var bulletEntity = state.EntityManager.Instantiate(entitiesReferences.BulletPrefabEntity);
			var nuzzleWorldTransform = SystemAPI.GetComponent<LocalToWorld>(shoot.ValueRO.NuzzleEntity);
			var bulletSpawnWorldPosition = nuzzleWorldTransform.Position;

			SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));
			var bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
			bullet.ValueRW.DamageAmount = shoot.ValueRO.DamageAmount;

			if (SystemAPI.HasComponent<LocalTransform>(target.ValueRO.TargetEntity))
			{
				var targetPosition = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.TargetEntity).Position;
				bullet.ValueRW.Direction = math.normalize(targetPosition - localTransform.ValueRO.Position);

				// if target has a target override, set the bullet's target entity to the target's target entity
				if (SystemAPI.HasComponent<TargetOverride>(target.ValueRO.TargetEntity))
				{
					var enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.TargetEntity);

					if (enemyTargetOverride.ValueRO.TargetEntity == Entity.Null)
					{
						enemyTargetOverride.ValueRW.TargetEntity = entity;
					}
				}
			}
			else
			{
				// Default forward Direction if target doesn't exist
				bullet.ValueRW.Direction = math.forward();
			}

			shoot.ValueRW.OnShoot.IsTriggered = true;
			shoot.ValueRW.OnShoot.ShootFromPosition = bulletSpawnWorldPosition;
		}
	}
}