using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

internal partial struct FindTargetSystem : ISystem
{
	private ComponentLookup<Unit> _unitLookup;
	private ComponentLookup<LocalTransform> _targetLocalTransformLookup;

	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
		_unitLookup = SystemAPI.GetComponentLookup<Unit>(true);
		_targetLocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
		_unitLookup.Update(ref state);
		_targetLocalTransformLookup.Update(ref state);

		var deltaTime = SystemAPI.Time.DeltaTime;

		var findTargetJob = new FindTargetJob
		                    {
			                    CollisionWorld = physicsWorldSingleton.CollisionWorld,
			                    UnitLookup = _unitLookup,
			                    TargetLocalTransformLookup = _targetLocalTransformLookup,
			                    DeltaTime = deltaTime
		                    };

		state.Dependency = findTargetJob.ScheduleParallel(state.Dependency);
	}
}

internal partial struct FindTargetJob : IJobEntity
{
	[ReadOnly] public CollisionWorld CollisionWorld;
	[ReadOnly] public ComponentLookup<Unit> UnitLookup;
	[ReadOnly] public ComponentLookup<LocalTransform> TargetLocalTransformLookup;
	[ReadOnly] public float DeltaTime;

	public void Execute(in LocalTransform localTransform, ref FindTarget findTarget, ref Target target, ref TargetOverride targetOverride)
	{
		findTarget.Timer -= DeltaTime;
		// Only search for targets when Timer expires
		if (findTarget.Timer <= 0f)
		{
			// Reset Timer
			findTarget.Timer = findTarget.Cooldown;

			if (targetOverride.TargetEntity != Entity.Null)
			{
				target.TargetEntity = targetOverride.TargetEntity;

				return;
			}

			var distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

			var collisionFilter = new CollisionFilter
			                      {
				                      BelongsTo = ~0u,
				                      CollidesWith = 1u << GameConfig.UNIT_LAYER,
				                      GroupIndex = 0
			                      };

			var closestTarget = Entity.Null;
			var closestDistanceSq = float.MaxValue;
			var currentTargetDistanceOffset = 0f;

			if (target.TargetEntity != Entity.Null)
			{
				closestTarget = target.TargetEntity;
				var targetLocalTransform = TargetLocalTransformLookup[closestTarget];
				closestDistanceSq = math.distancesq(localTransform.Position, targetLocalTransform.Position);
				currentTargetDistanceOffset = 5f;
			}

			if (CollisionWorld.OverlapSphere(localTransform.Position, findTarget.Range, ref distanceHitList, collisionFilter))
			{
				for (var i = 0; i < distanceHitList.Length; i++)
				{
					var distanceHit = distanceHitList[i];

					if (UnitLookup.HasComponent(distanceHit.Entity))
					{
						var hitUnit = UnitLookup[distanceHit.Entity];
						if (findTarget.TargetFaction == hitUnit.Faction)
						{
							var distanceSq = distanceHit.Distance * distanceHit.Distance;
							if (distanceSq + currentTargetDistanceOffset * currentTargetDistanceOffset < closestDistanceSq)
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