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

		var activeAnimationJob = new ActiveAnimationJob
		                         {
			                         DeltaTime = SystemAPI.Time.DeltaTime,
			                         AnimationDataBlobArrayBlobAssetReference = animationDataHolder.AnimationDataBlobArrayBlobAssetReference
		                         };

		activeAnimationJob.ScheduleParallel();
	}
}

[BurstCompile]
public partial struct ActiveAnimationJob : IJobEntity
{
	public float DeltaTime;
	public BlobAssetReference<BlobArray<AnimationData>> AnimationDataBlobArrayBlobAssetReference;

	public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
	{
		ref var animationData = ref AnimationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.ActiveAnimationType];

		activeAnimation.FrameTimer += DeltaTime;

		if (activeAnimation.FrameTimer > animationData.FrameTimerMax)
		{
			activeAnimation.FrameTimer -= animationData.FrameTimerMax;

			activeAnimation.Frame = (activeAnimation.Frame + 1) % animationData.FrameMax;

			materialMeshInfo.Mesh = animationData.intMeshIdBlobArray[activeAnimation.Frame];

			if (activeAnimation.Frame == 0 && AnimationDataSO.IsAnimationUninterruptible(activeAnimation.ActiveAnimationType))
			{
				activeAnimation.ActiveAnimationType = AnimationType.None;
			}
		}
	}
}