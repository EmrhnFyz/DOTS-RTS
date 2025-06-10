using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class VisualUnderFoWAuthoring : MonoBehaviour
{
	public GameObject parentGameObject;
	public float sphereCastSize;

	public class Baker : Baker<VisualUnderFoWAuthoring>
	{
		public override void Bake(VisualUnderFoWAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new VisualUnderFoW
			                     {
				                     IsVisible = true,
				                     ParentEntity = GetEntity(authoring.parentGameObject, TransformUsageFlags.Dynamic),
				                     SphereCastSize = authoring.sphereCastSize,
				                     Cooldown = GameConfig.VISUAL_UNDER_FOW_COOLDOWN
			                     });
			AddComponent(entity, new DisableRendering());
		}
	}
}

public struct VisualUnderFoW : IComponentData
{
	public bool IsVisible;
	public Entity ParentEntity;
	public float SphereCastSize;
	public float Timer;
	public float Cooldown;
}