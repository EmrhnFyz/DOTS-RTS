using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

internal partial struct VisualUnderFoWSystem : ISystem
{
	private ComponentLookup<LocalTransform> _localTransformLookup;


	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
		state.RequireForUpdate<PhysicsWorldSingleton>();
		state.RequireForUpdate<GameSceneTag>();

		_localTransformLookup = state.GetComponentLookup<LocalTransform>(true);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
		var collisionWorld = physicsWorldSingleton.CollisionWorld;

		var endSimulationEntityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

		_localTransformLookup.Update(ref state);

		var visualUnderFoWJob = new VisualUnderFogOfWarJob
		                        {
			                        EndSimulationEntityCommandBuffer = endSimulationEntityCommandBuffer.AsParallelWriter(),
			                        CollisionWorld = collisionWorld,
			                        LocalTransformLookup = _localTransformLookup,
			                        DeltaTime = SystemAPI.Time.DeltaTime
		                        };

		visualUnderFoWJob.ScheduleParallel();
	}

	[BurstCompile]
	public void OnDestroy(ref SystemState state)
	{
	}
}

[BurstCompile]
public partial struct VisualUnderFogOfWarJob : IJobEntity
{
	public EntityCommandBuffer.ParallelWriter EndSimulationEntityCommandBuffer;
	[ReadOnly] public CollisionWorld CollisionWorld;
	[ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;

	[ReadOnly] public float DeltaTime;

	public void Execute(ref VisualUnderFoW visualUnderFoW, [ChunkIndexInQuery] int chunkIndexInQuery, Entity entity)
	{
		visualUnderFoW.Timer -= DeltaTime;
		if (visualUnderFoW.Timer > 0)
		{
			return; // still in cooldown
		}

		visualUnderFoW.Timer += visualUnderFoW.Cooldown;

		var parentLocalTransform = LocalTransformLookup[visualUnderFoW.ParentEntity];

		if (!CollisionWorld.SphereCast(parentLocalTransform.Position, visualUnderFoW.SphereCastSize, new float3(0, 1, 0), 100, GameConfig.FOWCollisionFilter))
		{
			// not under fow
			if (visualUnderFoW.IsVisible)
			{
				visualUnderFoW.IsVisible = false;
				EndSimulationEntityCommandBuffer.AddComponent<DisableRendering>(chunkIndexInQuery, entity);
			}
		}
		else
		{
			// under fow
			if (!visualUnderFoW.IsVisible)
			{
				visualUnderFoW.IsVisible = true;
				EndSimulationEntityCommandBuffer.RemoveComponent<DisableRendering>(chunkIndexInQuery, entity);
			}
		}
	}
}