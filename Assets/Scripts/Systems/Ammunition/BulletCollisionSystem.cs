using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
internal partial struct BulletCollisionSystem : ISystem
{
	private ComponentLookup<Bullet> _bulletLookup;
	private ComponentLookup<Health> _healthLookup;
	private ComponentLookup<Friendly> _friendlyLookup;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
		state.RequireForUpdate<SimulationSingleton>();
		_bulletLookup = state.GetComponentLookup<Bullet>();
		_healthLookup = state.GetComponentLookup<Health>();
		_friendlyLookup = state.GetComponentLookup<Friendly>(true); // Read-only lookup for friendly entities
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entityCommandBuffer = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		_bulletLookup.Update(ref state);
		_healthLookup.Update(ref state);
		_friendlyLookup.Update(ref state);

		var collisionJob = new CollisionJob
		                   {
			                   EntityCommandBuffer = entityCommandBuffer,
			                   FriendlyLookup = _friendlyLookup,
			                   BulletLookup = _bulletLookup,
			                   HealthLookup = _healthLookup
		                   };

		var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
		state.Dependency = collisionJob.Schedule(simulation, state.Dependency);

		state.Dependency.Complete();
	}

	[BurstCompile]
	private struct CollisionJob : ITriggerEventsJob
	{
		[ReadOnly] public ComponentLookup<Friendly> FriendlyLookup;
		public EntityCommandBuffer EntityCommandBuffer;
		public ComponentLookup<Bullet> BulletLookup;
		public ComponentLookup<Health> HealthLookup;

		public void Execute(TriggerEvent triggerEvent)
		{
			var entityA = triggerEvent.EntityA;
			var entityB = triggerEvent.EntityB;

			if (BulletLookup.HasComponent(entityA))
			{
				if (!HealthLookup.HasComponent(entityB) || FriendlyLookup.HasComponent(entityB))
				{
					return;
				}

				var health = HealthLookup[entityB];
				health.currentHealth -= BulletLookup[entityA].damageAmount;
				HealthLookup[entityB] = health;

				EntityCommandBuffer.DestroyEntity(entityA);
			}
			else
			{
				if (!HealthLookup.HasComponent(entityA) || FriendlyLookup.HasComponent(entityA))
				{
					return;
				}

				var health = HealthLookup[entityA];
				health.currentHealth -= BulletLookup[entityB].damageAmount;
				HealthLookup[entityA] = health;

				EntityCommandBuffer.DestroyEntity(entityB);
			}
		}
	}
}