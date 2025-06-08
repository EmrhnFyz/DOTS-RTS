using UnityEngine;

[CreateAssetMenu(fileName = "ResourceHarvesterTypeSO", menuName = "ScriptableObjects/Buildings/ResourceHarvesterTypeSO")]
public class ResourceHarvesterTypeSO : BuildingTypeSO
{
    public ResourceType resourceType;
    public int harvestDistance;
}
