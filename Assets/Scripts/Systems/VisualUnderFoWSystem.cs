using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

internal partial struct VisualUnderFoWSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var collisionWorld = physicsWorldSingleton.CollisionWorld;

        var endSimulationEntityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (visualUnderFoW, entity) in SystemAPI.Query<RefRW<VisualUnderFoW>>().WithEntityAccess())
        {
            var parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(visualUnderFoW.ValueRO.ParentEntity);

            if (!collisionWorld.SphereCast(parentLocalTransform.Position, visualUnderFoW.ValueRO.sphereCastSize, new float3(0, 1, 0), 100, GameConfig.FOWCollisionFilter))
            {
                // not under fow
                if (visualUnderFoW.ValueRO.IsVisible)
                {
                    visualUnderFoW.ValueRW.IsVisible = false;
                    endSimulationEntityCommandBuffer.AddComponent<DisableRendering>(entity);
                }
            }
            else
            {
                // under fow
                if (!visualUnderFoW.ValueRO.IsVisible)
                {
                    visualUnderFoW.ValueRW.IsVisible = true;
                    endSimulationEntityCommandBuffer.RemoveComponent<DisableRendering>(entity);
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}