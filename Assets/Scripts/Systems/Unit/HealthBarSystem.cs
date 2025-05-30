using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

internal partial struct HealthBarSystem : ISystem
{
	private ComponentLookup<LocalTransform> _localTransformLookup;
	private ComponentLookup<Health> _healthLookup;
	private ComponentLookup<PostTransformMatrix> _postTransformMatrixLookup;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		_localTransformLookup = state.GetComponentLookup<LocalTransform>();
		_healthLookup = state.GetComponentLookup<Health>(true);
		_postTransformMatrixLookup = state.GetComponentLookup<PostTransformMatrix>();
	}

	public void OnUpdate(ref SystemState state)
	{
		var cameraForward = GameConfig.GetCameraForward();
		_localTransformLookup.Update(ref state);
		_healthLookup.Update(ref state);
		_postTransformMatrixLookup.Update(ref state);

		var healthBarJob = new HealthBarJob
		                   {
			                   LocalTransformLookup = _localTransformLookup,
			                   HealthLookup = _healthLookup,
			                   PostTransformMatrixLookup = _postTransformMatrixLookup,
			                   CameraForward = cameraForward
		                   };

		healthBarJob.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct HealthBarJob : IJobEntity
{
	[NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> LocalTransformLookup;
	[ReadOnly] public ComponentLookup<Health> HealthLookup;
	[NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;

	public float3 CameraForward;

	public void Execute(in HealthBar healthBar, Entity entity)
	{
		var localTransform = LocalTransformLookup.GetRefRW(entity);

		if (localTransform.ValueRO.Scale != 0f)
		{
			var healthBarLocalTransform = LocalTransformLookup[healthBar.healthEntity];
			localTransform.ValueRW.Rotation = healthBarLocalTransform.InverseTransformRotation(quaternion.LookRotation(CameraForward, math.up()));
		}

		var health = HealthLookup[healthBar.healthEntity];

		if (!health.OnHealthChanged)
		{
			return;
		}

		var healthNormalized = health.CurrentHealth / health.MaxHealth;

		localTransform.ValueRW.Scale = Mathf.Approximately(healthNormalized, 1f) ? 0f : 1f;

		var healthBarHolderPostTransformMatrix = PostTransformMatrixLookup.GetRefRW(healthBar.barHolderEntity);
		healthBarHolderPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
	}
}