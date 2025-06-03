using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

internal partial struct MeleeAttackSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
		var collisionWorld = physicsWorldSingleton.CollisionWorld;
		var raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);
		foreach (var (localTransform, target, meleeAttack, unitMover)
		         in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Target>, RefRW<MeleeAttack>, RefRW<UnitMover>>().WithDisabled<MoveOverride>())
		{
			if (target.ValueRO.TargetEntity == Entity.Null)
			{
				continue;
			}

			var targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.TargetEntity);
			var distanceToTarget = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
			var isCloseEnoughToAttack = distanceToTarget < meleeAttack.ValueRO.AttackDistance;
			var isTouchingTarget = false;

			if (!isCloseEnoughToAttack)
			{
				var directionToTarget = math.normalize(targetLocalTransform.Position - localTransform.ValueRO.Position);
				var offset = 0.1f;
				var raycastInput = new RaycastInput
				                   {
					                   Start = localTransform.ValueRO.Position,
					                   End = localTransform.ValueRO.Position + directionToTarget * (meleeAttack.ValueRO.ColliderSize + offset),
					                   Filter = CollisionFilter.Default
				                   };
				raycastHitList.Clear();
				if (collisionWorld.CastRay(raycastInput, ref raycastHitList))
				{
					foreach (var raycastHit in raycastHitList)
					{
						if (raycastHit.Entity != target.ValueRO.TargetEntity)
						{
							continue;
						}

						isTouchingTarget = true;
						break;
					}
				}
			}

			if (!isCloseEnoughToAttack && !isTouchingTarget)
			{
				unitMover.ValueRW.TargetPosition = targetLocalTransform.Position;
			}
			else
			{
				unitMover.ValueRW.TargetPosition = localTransform.ValueRO.Position;

				meleeAttack.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

				if (meleeAttack.ValueRO.Timer > 0f)
				{
					continue;
				}

				meleeAttack.ValueRW.Timer = meleeAttack.ValueRO.Cooldown;

				var targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.TargetEntity);
				targetHealth.ValueRW.CurrentHealth -= meleeAttack.ValueRO.DamageAmount;
				targetHealth.ValueRW.OnHealthChanged = true;

				meleeAttack.ValueRW.OnAttacked = true;
			}
		}
	}
}