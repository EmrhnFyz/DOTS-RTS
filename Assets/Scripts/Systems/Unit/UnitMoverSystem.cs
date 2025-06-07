using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

internal partial struct UnitMoverSystem : ISystem
{
	public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueuedLookup;
	public ComponentLookup<FlowFieldFollower> flowFieldFollowerLookup;
	public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestLookup;
	public ComponentLookup<MoveOverride> moveOverrideLookup;
	public ComponentLookup<GridSystem.GridNode> GridNodeLookup;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldSingleton>();
		state.RequireForUpdate<GridSystem.GridSystemData>();
		targetPositionPathQueuedLookup = state.GetComponentLookup<TargetPositionPathQueued>(false);
		flowFieldFollowerLookup = state.GetComponentLookup<FlowFieldFollower>(false);
		flowFieldPathRequestLookup = state.GetComponentLookup<FlowFieldPathRequest>(false);
		moveOverrideLookup = state.GetComponentLookup<MoveOverride>(false);
		GridNodeLookup = state.GetComponentLookup<GridSystem.GridNode>(false);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();
		var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
		var collisionWorld = physicsWorldSingleton.CollisionWorld;

		targetPositionPathQueuedLookup.Update(ref state);
		flowFieldFollowerLookup.Update(ref state);
		flowFieldPathRequestLookup.Update(ref state);
		moveOverrideLookup.Update(ref state);
		GridNodeLookup.Update(ref state);

		var targetPositionPathQueuedJob = new TargetPositionPathQueuedJob
		{
			CollisionWorld = collisionWorld,
			width = gridSystemData.Width,
			height = gridSystemData.Height,
			gridCostArray = gridSystemData.GridCostArray,
			cellSize = gridSystemData.CellSize,
			TargetPositionPathQueuedLookup = targetPositionPathQueuedLookup,
			FlowFieldFollowerLookup = flowFieldFollowerLookup,
			FlowFieldPathRequestLookup = flowFieldPathRequestLookup,
			MoveOverrideLookup = moveOverrideLookup
		};
		targetPositionPathQueuedJob.ScheduleParallel();

		var testCanMoveStraightJob = new TestCanMoveStraightJob
		{
			CollisionWorld = collisionWorld,
			FlowFieldFollowerLookup = flowFieldFollowerLookup
		};
		testCanMoveStraightJob.ScheduleParallel();

		var flowFieldFollowerJob = new FlowFieldFollowerJob
		{
			TotalGridMapEntityArray = gridSystemData.TotalGridMapEntityArray,
			CellSize = gridSystemData.CellSize,
			Width = gridSystemData.Width,
			Height = gridSystemData.Height,
			FlowFieldFollowerLookup = flowFieldFollowerLookup,
			GridNodeLookup = GridNodeLookup
		};
		flowFieldFollowerJob.ScheduleParallel();


		var unitMoverJob = new UnitMoverJob
		{
			DeltaTime = SystemAPI.Time.DeltaTime
		};
		unitMoverJob.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
	public float DeltaTime;

	public void Execute(ref LocalTransform localTransform, ref UnitMover unitMoverComponent, ref PhysicsVelocity physicsVelocity)
	{
		var moveDirection = unitMoverComponent.TargetPosition - localTransform.Position;

		if (math.lengthsq(moveDirection) <= GameConfig.REACH_TARGET_DISTANCE_SQ)
		{
			physicsVelocity.Linear = float3.zero;
			physicsVelocity.Angular = float3.zero;
			unitMoverComponent.IsMoving = false;
			return;
		}

		unitMoverComponent.IsMoving = true;

		var lookRotation = quaternion.LookRotation(moveDirection, math.up());
		moveDirection = math.lengthsq(moveDirection) > 0.0001f ? math.normalize(moveDirection) : float3.zero;

		localTransform.Rotation = math.slerp(localTransform.Rotation,
											 lookRotation,
											 DeltaTime * unitMoverComponent.RotationSpeed);
		physicsVelocity.Linear = moveDirection * unitMoverComponent.MoveSpeed;
		physicsVelocity.Angular = float3.zero;
	}
}

[BurstCompile]
[WithAll(typeof(TargetPositionPathQueued))]
public partial struct TargetPositionPathQueuedJob : IJobEntity
{
	[NativeDisableParallelForRestriction] public ComponentLookup<TargetPositionPathQueued> TargetPositionPathQueuedLookup;
	[NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> FlowFieldFollowerLookup;
	[NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldPathRequest> FlowFieldPathRequestLookup;

	[NativeDisableParallelForRestriction] public ComponentLookup<MoveOverride> MoveOverrideLookup;

	[ReadOnly] public CollisionWorld CollisionWorld;

	[ReadOnly] public int width;
	[ReadOnly] public int height;
	[ReadOnly] public NativeArray<byte> gridCostArray;
	[ReadOnly] public float cellSize;

	public void Execute(in LocalTransform localTransform,
						ref UnitMover unitMover,
						Entity entity)
	{
		var raycastInput = new RaycastInput
		{
			Start = localTransform.Position,
			End = TargetPositionPathQueuedLookup[entity].TargetPosition,
			Filter = GameConfig.PathfindingWallCollisionFilter
		};

		if (!CollisionWorld.CastRay(raycastInput))
		{
			// No wall in the way, proceed with the target position
			unitMover.TargetPosition = TargetPositionPathQueuedLookup[entity].TargetPosition;
			FlowFieldPathRequestLookup.SetComponentEnabled(entity, false);
			FlowFieldFollowerLookup.SetComponentEnabled(entity, false);
		}
		else
		{

			if (MoveOverrideLookup.HasComponent(entity))
			{
				MoveOverrideLookup.SetComponentEnabled(entity, false);
			}

			if (GridSystem.IsValidWalkablePosition(TargetPositionPathQueuedLookup[entity].TargetPosition, width, height, gridCostArray, cellSize))
			{
				var flowFieldPathRequest = FlowFieldPathRequestLookup[entity];
				flowFieldPathRequest.TargetPosition = TargetPositionPathQueuedLookup[entity].TargetPosition;
				FlowFieldPathRequestLookup[entity] = flowFieldPathRequest;
				FlowFieldPathRequestLookup.SetComponentEnabled(entity, true);
			}
			else
			{
				unitMover.TargetPosition = localTransform.Position;
				FlowFieldPathRequestLookup.SetComponentEnabled(entity, false);
				FlowFieldFollowerLookup.SetComponentEnabled(entity, false);
			}
		}

		TargetPositionPathQueuedLookup.SetComponentEnabled(entity, false);
	}
}

[BurstCompile]
[WithAll(typeof(FlowFieldFollower))]
public partial struct TestCanMoveStraightJob : IJobEntity
{
	[NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> FlowFieldFollowerLookup;
	[ReadOnly] public CollisionWorld CollisionWorld;

	public void Execute(in LocalTransform localTransform, ref UnitMover unitMover, Entity entity)
	{
		var flowFieldFollower = FlowFieldFollowerLookup[entity];

		var raycastInput = new RaycastInput
		{
			Start = localTransform.Position,
			End = flowFieldFollower.TargetPosition,
			Filter = GameConfig.PathfindingWallCollisionFilter
		};

		if (!CollisionWorld.CastRay(raycastInput))
		{
			// No wall in the way, proceed with the target position
			unitMover.TargetPosition = flowFieldFollower.TargetPosition;
			FlowFieldFollowerLookup.SetComponentEnabled(entity, false);
		}
	}
}

[BurstCompile]
[WithAll(typeof(FlowFieldFollower))]
public partial struct FlowFieldFollowerJob : IJobEntity
{
	[NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> FlowFieldFollowerLookup;
	[ReadOnly] public ComponentLookup<GridSystem.GridNode> GridNodeLookup;

	[ReadOnly] public NativeArray<Entity> TotalGridMapEntityArray;
	[ReadOnly] public float CellSize;
	[ReadOnly] public int Width;
	[ReadOnly] public int Height;
	public void Execute(in LocalTransform localTransform, ref UnitMover unitMover, Entity entity)
	{
		var flowFieldFollower = FlowFieldFollowerLookup[entity];
		var gridPosition = GridSystem.GetGridPosition(localTransform.Position, CellSize);
		var index = GridSystem.CalculateIndex(gridPosition, Width);
		int totalCount = Width * Height;

		var gridNodeEntity = TotalGridMapEntityArray[flowFieldFollower.GridIndex * totalCount + index];
		var gridNode = GridNodeLookup[gridNodeEntity];
		var gridNodeMoveVector = GridSystem.GetWorldMovementVector(gridNode.Vector);

		if (GridSystem.IsWall(gridNode))
		{
			gridNodeMoveVector = flowFieldFollower.LastMoveVector;
		}
		else
		{
			flowFieldFollower.LastMoveVector = gridNodeMoveVector;
		}

		unitMover.TargetPosition = GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, CellSize) + gridNodeMoveVector * CellSize * 2f;

		if (math.distance(localTransform.Position, flowFieldFollower.TargetPosition) < CellSize * 0.5f)
		{
			unitMover.TargetPosition = localTransform.Position;
			FlowFieldFollowerLookup.SetComponentEnabled(entity, false);
		}

		FlowFieldFollowerLookup[entity] = flowFieldFollower;
	}
}
