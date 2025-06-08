using System.Collections.Generic;
using UnityEngine;
using UnityEventKit;

public class ResourceManager : MonoBehaviour
{
    public readonly struct OnResourceUpdated : IEvent
    {
        public readonly ResourceType ResourceType;
        public readonly int Amount;

        public OnResourceUpdated(ResourceType resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
        }
    }

    public static ResourceManager Instance { get; private set; }

    [SerializeField] private ResourceTypeListSO resourceTypeListSO;
    private Dictionary<ResourceType, int> resourceTypeAmountDictionary;

    private void Awake()
    {
        Instance = this;

        resourceTypeAmountDictionary = new Dictionary<ResourceType, int>();
        foreach (var resourceType in resourceTypeListSO.resourceTypeSOList)
        {
            resourceTypeAmountDictionary.Add(resourceType.ResourceType, 0);
        }
    }
    void OnDestroy()
    {

    }

    public void AddResource(ResourceType resourceType, int amount)
    {
        resourceTypeAmountDictionary[resourceType] += amount;

        EventBus.Global.Publish(new OnResourceUpdated(resourceType, resourceTypeAmountDictionary[resourceType]));
    }

    public void RemoveResource(ResourceType resourceType, int amount)
    {
        resourceTypeAmountDictionary[resourceType] -= amount;
    }

}
