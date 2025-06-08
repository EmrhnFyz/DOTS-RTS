using Unity.Entities;
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
                sphereCastSize = authoring.sphereCastSize
            });
        }
    }
}

public struct VisualUnderFoW : IComponentData
{
    public bool IsVisible;
    public Entity ParentEntity;
    public float sphereCastSize;
}