using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationDataHolderAuthoring : MonoBehaviour
{
	public AnimationDataListSO animationDataListSO;

	public class Baker : Baker<AnimationDataHolderAuthoring>
	{
		public override void Bake(AnimationDataHolderAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			var entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

			var animationDataHolder = new AnimationDataHolder();

			BlobBuilder blobBuilder = new(Allocator.Temp);
			ref var animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();
			var animationDataBlobBuilderArray = blobBuilder.Allocate(ref animationDataBlobArray, Enum.GetValues(typeof(AnimationType)).Length);

			var index = 0;
			foreach (AnimationType animationType in Enum.GetValues(typeof(AnimationType)))
			{
				var animationDataSO = authoring.animationDataListSO.GetAnimationDataSO(animationType);
				var blobBuilderArray = blobBuilder.Allocate(ref animationDataBlobBuilderArray[index].BatchMeshIdBlobArray, animationDataSO.meshArray.Length);

				animationDataBlobBuilderArray[index].FrameTimerMax = animationDataSO.frameTimerMax;
				animationDataBlobBuilderArray[index].FrameMax = animationDataSO.meshArray.Length;

				for (var i = 0; i < animationDataSO.meshArray.Length; i++)
				{
					var mesh = animationDataSO.meshArray[i];
					blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
				}

				index++;
			}


			animationDataHolder.AnimationDataBlobArrayBlobAssetReference = blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
			blobBuilder.Dispose();
			AddBlobAsset(ref animationDataHolder.AnimationDataBlobArrayBlobAssetReference, out _);
			AddComponent(entity, animationDataHolder);
		}
	}
}

public struct AnimationDataHolder : IComponentData
{
	public BlobAssetReference<BlobArray<AnimationData>> AnimationDataBlobArrayBlobAssetReference;
}

public struct AnimationData
{
	public float FrameTimerMax;
	public int FrameMax;
	public BlobArray<BatchMeshID> BatchMeshIdBlobArray;
}