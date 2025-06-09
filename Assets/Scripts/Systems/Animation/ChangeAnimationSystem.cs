using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

[UpdateBefore(typeof(ActiveAnimationSystem))]
internal partial struct ChangeAnimationSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<AnimationDataHolder>();
		state.RequireForUpdate<GameSceneTag>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

		var changeAnimationJob = new ChangeAnimationJob
		                         {
			                         AnimationDataBlobArrayBlobAssetReference = animationDataHolder.AnimationDataBlobArrayBlobAssetReference
		                         };

		changeAnimationJob.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct ChangeAnimationJob : IJobEntity
{
	public BlobAssetReference<BlobArray<AnimationData>> AnimationDataBlobArrayBlobAssetReference;

	public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
	{
		if (AnimationDataSO.IsAnimationUninterruptible(activeAnimation.ActiveAnimationType))
		{
			return;
		}

		if (activeAnimation.ActiveAnimationType != activeAnimation.NextAnimationType)
		{
			activeAnimation.Frame = 0;
			activeAnimation.FrameTimer = 0f;
			activeAnimation.ActiveAnimationType = activeAnimation.NextAnimationType;

			ref var animationData = ref AnimationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.ActiveAnimationType];
			materialMeshInfo.Mesh = animationData.intMeshIdBlobArray[0];
		}
	}
}