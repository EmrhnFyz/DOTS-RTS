using Unity.Entities;
using UnityEngine;

public class AnimatedMeshAuthoring : MonoBehaviour
{
	public GameObject meshGameObject;

	public class Baker : Baker<AnimatedMeshAuthoring>
	{
		public override void Bake(AnimatedMeshAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new AnimatedMesh
			                     {
				                     MeshEntity = GetEntity(authoring.meshGameObject, TransformUsageFlags.Dynamic)
			                     });
		}
	}
}

public struct AnimatedMesh : IComponentData
{
	public Entity MeshEntity;
}