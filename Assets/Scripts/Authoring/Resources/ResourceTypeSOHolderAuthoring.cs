using Unity.Entities;
using UnityEngine;

public class ResourceTypeSOHolderAuthoring : MonoBehaviour
{
    public ResourceType resourceType;

    public class Baker : Baker<ResourceTypeSOHolderAuthoring>
    {
        public override void Bake(ResourceTypeSOHolderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ResourceTypeSOHolder
            {
                ResourceType = authoring.resourceType
            });
        }
    }
}

public struct ResourceTypeSOHolder : IComponentData
{
    public ResourceType ResourceType;
}