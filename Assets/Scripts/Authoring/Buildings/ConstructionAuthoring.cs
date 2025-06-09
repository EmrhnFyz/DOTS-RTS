using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ConstructionAuthoring : MonoBehaviour
{
    public class Baker : Baker<ConstructionAuthoring>
    {
        public override void Bake(ConstructionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Construction());
        }
    }
}

public struct Construction : IComponentData
{
    public float ConstructionTimer;
    public float ConstructionTimeMax;

    public float3 StartPosition;
    public float3 EndPosition;

    public BuildingType BuildingType;

    public Entity FinalPrefabEntity;
    public Entity VisualEntity;
}