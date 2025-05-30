using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

internal partial struct ActiveAnimationSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<AnimationDataHolder>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

		foreach (var (activeAnimation, materialMeshInfo) in SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>())
		{
			activeAnimation.ValueRW.FrameTimer += SystemAPI.Time.DeltaTime;

			ref var animationData = ref animationDataHolder.AnimationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.ValueRW.ActiveAnimationType];

			if (activeAnimation.ValueRW.FrameTimer > animationData.FrameTimerMax)
			{
				activeAnimation.ValueRW.FrameTimer -= animationData.FrameTimerMax;

				activeAnimation.ValueRW.Frame = (activeAnimation.ValueRW.Frame + 1) % animationData.FrameMax;

				materialMeshInfo.ValueRW.MeshID = animationData.BatchMeshIdBlobArray[activeAnimation.ValueRO.Frame];

				if (activeAnimation.ValueRO is { Frame: 0, ActiveAnimationType: AnimationType.SoldierShoot })
				{
					activeAnimation.ValueRW.ActiveAnimationType = AnimationType.None;
				}

				if (activeAnimation.ValueRO is { Frame: 0, ActiveAnimationType: AnimationType.ZombieAttack })
				{
					activeAnimation.ValueRW.ActiveAnimationType = AnimationType.None;
				}
			}
		}
	}
}