using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

internal partial struct UnitMoverSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var unitMoverJob = new UnitMoverJob
		                   {
			                   deltaTime = SystemAPI.Time.DeltaTime
		                   };
		unitMoverJob.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
	public float deltaTime;

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
		                                     deltaTime * unitMoverComponent.RotationSpeed);
		physicsVelocity.Linear = moveDirection * unitMoverComponent.MoveSpeed;
		physicsVelocity.Angular = float3.zero;
	}
}