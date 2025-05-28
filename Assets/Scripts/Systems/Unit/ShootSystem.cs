using Unity.Burst;
using Unity.Entities;
using UnityEngine;

internal partial struct ShootSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (shoot, target) in SystemAPI.Query<RefRW<Shoot>, RefRO<Target>>())
		{
			if (target.ValueRO.targetEntity == Entity.Null)
			{
				continue;
			}

			shoot.ValueRW.timer -= SystemAPI.Time.DeltaTime;

			if (shoot.ValueRO.timer > 0f)
			{
				continue;
			}

			shoot.ValueRW.timer = shoot.ValueRO.cooldown;
			var targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
			targetHealth.ValueRW.currentHealth -= shoot.ValueRO.damage;
			Debug.Log($"BOOM! to {target.ValueRO.targetEntity} CurrentHealth: {targetHealth.ValueRW.currentHealth} / {targetHealth.ValueRW.maxHealth}");
		}
	}
}