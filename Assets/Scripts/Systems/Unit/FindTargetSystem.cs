using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

internal partial struct FindTargetSystem : ISystem
{
	private ComponentLookup<Faction> _factionLookup;
	private ComponentLookup<LocalTransform> _targetLocalTransformLookup;

	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
		state.RequireForUpdate<GameSceneTag>();

		_factionLookup = SystemAPI.GetComponentLookup<Faction>(true);
		_targetLocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
		_factionLookup.Update(ref state);
		_targetLocalTransformLookup.Update(ref state);

		var deltaTime = SystemAPI.Time.DeltaTime;

		var findTargetJob = new FindTargetJob
		                    {
			                    CollisionWorld = physicsWorldSingleton.CollisionWorld,
			                    FactionLookup = _factionLookup,
			                    TargetLocalTransformLookup = _targetLocalTransformLookup,
			                    DeltaTime = deltaTime
		                    };

		state.Dependency = findTargetJob.ScheduleParallel(state.Dependency);
	}
}

internal partial struct FindTargetJob : IJobEntity
{
	[ReadOnly] public CollisionWorld CollisionWorld;
	[ReadOnly] public ComponentLookup<Faction> FactionLookup;
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

			var collisionFilter = GameConfig.FactionSelectionCollisionFilter;

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

					if (FactionLookup.HasComponent(distanceHit.Entity))
					{
						var hitUnit = FactionLookup[distanceHit.Entity];
						if (findTarget.TargetFaction == hitUnit.FactionType)
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