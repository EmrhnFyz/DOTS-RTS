using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

internal partial struct FindTargetSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
		var deltaTime = SystemAPI.Time.DeltaTime;

		var findTargetJob = new FindTargetJob
		                    {
			                    collisionWorld = physicsWorldSingleton.CollisionWorld,
			                    unitLookup = SystemAPI.GetComponentLookup<Unit>(true), // true = read-only

			                    deltaTime = deltaTime
		                    };

		state.Dependency = findTargetJob.ScheduleParallel(state.Dependency);
	}
}

internal partial struct FindTargetJob : IJobEntity
{
	[ReadOnly] public CollisionWorld collisionWorld;
	[ReadOnly] public ComponentLookup<Unit> unitLookup;
	[ReadOnly] public float deltaTime;

	public void Execute(in LocalTransform localTransform, ref FindTarget findTarget, ref Target target)
	{
		findTarget.Timer -= deltaTime;
		// Only search for targets when Timer expires
		if (findTarget.Timer <= 0f)
		{
			// Reset Timer
			findTarget.Timer = findTarget.Cooldown;

			var distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

			var collisionFilter = new CollisionFilter
			                      {
				                      BelongsTo = ~0u,
				                      CollidesWith = 1u << GameConfig.UNIT_LAYER,
				                      GroupIndex = 0
			                      };

			if (collisionWorld.OverlapSphere(localTransform.Position, findTarget.Range, ref distanceHitList, collisionFilter))
			{
				var closestTarget = Entity.Null;
				var closestDistanceSq = float.MaxValue;

				for (var i = 0; i < distanceHitList.Length; i++)
				{
					var distanceHit = distanceHitList[i];

					if (unitLookup.HasComponent(distanceHit.Entity))
					{
						var hitUnit = unitLookup[distanceHit.Entity];
						if (findTarget.TargetFaction == hitUnit.Faction)
						{
							var distanceSq = distanceHit.Distance * distanceHit.Distance;
							if (distanceSq < closestDistanceSq)
							{
								closestDistanceSq = distanceSq;
								closestTarget = distanceHit.Entity;
							}
						}
					}
				}

				target.TargetEntity = closestTarget;
			}

			distanceHitList.Dispose();
		}
	}
}