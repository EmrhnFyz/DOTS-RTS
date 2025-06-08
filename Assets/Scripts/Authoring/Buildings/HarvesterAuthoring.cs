using Unity.Entities;
using UnityEngine;

public class HarvesterAuthoring : MonoBehaviour
{
    public ResourceType resourceType;
    public float cooldown;
    public int harvestRate;

    public class Baker : Baker<HarvesterAuthoring>
    {
        public override void Bake(HarvesterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Harvester
            {
                ResourceType = authoring.resourceType,
                Cooldown = authoring.cooldown,
                HarvestRate = authoring.harvestRate
            });
        }
    }
}

public struct Harvester : IComponentData
{
    public ResourceType ResourceType;
    public float HarvestTimer;
    public float Cooldown;

    public int HarvestRate;
}