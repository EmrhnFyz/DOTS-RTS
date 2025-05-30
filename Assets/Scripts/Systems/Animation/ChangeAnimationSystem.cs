using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

[UpdateBefore(typeof(ActiveAnimationSystem))]
internal partial struct ChangeAnimationSystem : ISystem
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
			if (activeAnimation.ValueRO.ActiveAnimationType == AnimationType.SoldierShoot)
			{
				continue;
			}

			if (activeAnimation.ValueRO.ActiveAnimationType == AnimationType.ZombieAttack)
			{
				continue;
			}

			if (activeAnimation.ValueRO.ActiveAnimationType != activeAnimation.ValueRO.NextAnimationType)
			{
				activeAnimation.ValueRW.Frame = 0;
				activeAnimation.ValueRW.FrameTimer = 0f;
				activeAnimation.ValueRW.ActiveAnimationType = activeAnimation.ValueRO.NextAnimationType;

				ref var animationData = ref animationDataHolder.AnimationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.ValueRO.ActiveAnimationType];
				materialMeshInfo.ValueRW.MeshID = animationData.BatchMeshIdBlobArray[0];
			}
		}
	}
}