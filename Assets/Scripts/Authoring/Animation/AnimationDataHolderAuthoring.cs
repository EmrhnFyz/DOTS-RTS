using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class AnimationDataHolderAuthoring : MonoBehaviour
{
	public AnimationDataListSO animationDataListSO;
	public Material defaultMaterial;

	public class Baker : Baker<AnimationDataHolderAuthoring>
	{
		public override void Bake(AnimationDataHolderAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);

			var animationDataHolder = new AnimationDataHolder();

			var index = 0;
			foreach (AnimationType animationType in Enum.GetValues(typeof(AnimationType)))
			{
				var animationDataSO = authoring.animationDataListSO.GetAnimationDataSO(animationType);
				for (var i = 0; i < animationDataSO.meshArray.Length; i++)
				{
					var mesh = animationDataSO.meshArray[i];
					var additionalEntity = CreateAdditionalEntity(TransformUsageFlags.None, true);
					AddComponent(additionalEntity, new MaterialMeshInfo());
					AddComponent(additionalEntity, new RenderMeshUnmanaged
					                               {
						                               materialForSubMesh = authoring.defaultMaterial,
						                               mesh = mesh
					                               });

					AddComponent(additionalEntity, new AnimationDataHolderSubEntity
					                               {
						                               AnimationType = animationType,
						                               MeshIndex = i
					                               });
				}

				index++;
			}

			AddComponent(entity, new AnimationDataHolderObjectData
			                     {
				                     animationDataListSO = authoring.animationDataListSO
			                     });

			AddComponent(entity, animationDataHolder);
		}
	}
}

public struct AnimationDataHolderObjectData : IComponentData
{
	public UnityObjectRef<AnimationDataListSO> animationDataListSO;
}

public struct AnimationDataHolderSubEntity : IComponentData
{
	public AnimationType AnimationType;
	public int MeshIndex;
}

public struct AnimationDataHolder : IComponentData
{
	public BlobAssetReference<BlobArray<AnimationData>> AnimationDataBlobArrayBlobAssetReference;
}

public struct AnimationData
{
	public float FrameTimerMax;
	public int FrameMax;
	public BlobArray<int> intMeshIdBlobArray;
}