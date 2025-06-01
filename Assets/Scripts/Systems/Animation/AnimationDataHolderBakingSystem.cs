using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[UpdateInGroup(typeof(PostBakingSystemGroup))]
internal partial struct AnimationDataHolderBakingSystem : ISystem
{
	public void OnUpdate(ref SystemState state)
	{
		AnimationDataListSO animationDataListSO = null;
		foreach (var animationDataHolderObjectData in SystemAPI.Query<RefRO<AnimationDataHolderObjectData>>())
		{
			animationDataListSO = animationDataHolderObjectData.ValueRO.animationDataListSO.Value;
		}

		Dictionary<AnimationType, int[]> blobAssetDataDictionary = new();
		foreach (AnimationType animationType in Enum.GetValues(typeof(AnimationType)))
		{
			var animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
			blobAssetDataDictionary[animationType] = new int[animationDataSO.meshArray.Length];
		}

		foreach (var (animationDataHolderSubEntity, materialMeshInfo) in SystemAPI.Query<RefRO<AnimationDataHolderSubEntity>, RefRO<MaterialMeshInfo>>())
		{
			blobAssetDataDictionary[animationDataHolderSubEntity.ValueRO.AnimationType][animationDataHolderSubEntity.ValueRO.MeshIndex] = materialMeshInfo.ValueRO.Mesh;
		}

		foreach (var animationDataHolder in SystemAPI.Query<RefRW<AnimationDataHolder>>())
		{
			// Dispose the old blob asset reference if it exists
			if (animationDataHolder.ValueRW.AnimationDataBlobArrayBlobAssetReference.IsCreated)
			{
				animationDataHolder.ValueRW.AnimationDataBlobArrayBlobAssetReference.Dispose();
			}

			BlobBuilder blobBuilder = new(Allocator.Temp);
			ref var animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();
			var animationDataBlobBuilderArray = blobBuilder.Allocate(ref animationDataBlobArray, Enum.GetValues(typeof(AnimationType)).Length);

			var index = 0;
			foreach (AnimationType animationType in Enum.GetValues(typeof(AnimationType)))
			{
				var animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);

				var blobBuilderArray = blobBuilder.Allocate(ref animationDataBlobBuilderArray[index].intMeshIdBlobArray, animationDataSO.meshArray.Length);

				animationDataBlobBuilderArray[index].FrameTimerMax = animationDataSO.frameTimerMax;
				animationDataBlobBuilderArray[index].FrameMax = animationDataSO.meshArray.Length;

				for (var i = 0; i < animationDataSO.meshArray.Length; i++)
				{
					blobBuilderArray[i] = blobAssetDataDictionary[animationType][i];
				}

				index++;
			}


			animationDataHolder.ValueRW.AnimationDataBlobArrayBlobAssetReference = blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
			blobBuilder.Dispose();
		}
	}
}