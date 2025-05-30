using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

internal partial struct HealthBarSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var cameraForward = GameConfig.GetCameraForward();

		foreach (var (healthBar, localTransform) in SystemAPI.Query<RefRO<HealthBar>, RefRW<LocalTransform>>())
		{
			if (localTransform.ValueRO.Scale != 0f)
			{
				var healthBarLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
				localTransform.ValueRW.Rotation = healthBarLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
			}

			var health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

			if (!health.OnHealthChanged)
			{
				continue;
			}

			var healthNormalized = health.CurrentHealth / health.MaxHealth;

			localTransform.ValueRW.Scale = Mathf.Approximately(healthNormalized, 1f) ? 0f : 1f;

			var healthBarHolderPostTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barHolderEntity);
			healthBarHolderPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
		}
	}
}