using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

internal partial struct UnitMoverSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<GridSystem.GridSystemData>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();
		foreach (var (localTransform, flowFieldFollower, enabledFlowFieldFollower, unitMover)
		         in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>, RefRW<UnitMover>>())
		{
			var gridPosition = GridSystem.GetGridPosition(localTransform.ValueRO.Position, gridSystemData.CellSize);
			var index = GridSystem.CalculateIndex(gridPosition, gridSystemData.Width);
			var gridNodeEntity = gridSystemData.GridMapArray[flowFieldFollower.ValueRO.GridIndex].GridEntityArray[index];
			var gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridNodeEntity);
			var gridNodeMoveVector = GridSystem.GetWorldMovementVector(gridNode.Vector);

			if (GridSystem.IsWall(gridNode))
			{
				gridNodeMoveVector = flowFieldFollower.ValueRO.LastMoveVector;
			}
			else
			{
				flowFieldFollower.ValueRW.LastMoveVector = gridNodeMoveVector;
			}

			unitMover.ValueRW.TargetPosition = GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridSystemData.CellSize) + gridNodeMoveVector * gridSystemData.CellSize * 2f;

			if (math.distance(localTransform.ValueRO.Position, flowFieldFollower.ValueRO.TargetPosition) < gridSystemData.CellSize * 0.5f)
			{
				unitMover.ValueRW.TargetPosition = localTransform.ValueRO.Position;
				enabledFlowFieldFollower.ValueRW = false;
			}
		}

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