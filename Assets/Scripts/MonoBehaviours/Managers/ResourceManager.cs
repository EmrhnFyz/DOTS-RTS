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

        AddResource(ResourceType.Gold, 100);
        AddResource(ResourceType.Iron, 100);
        AddResource(ResourceType.Oil, 100);
    }

    public void AddResource(ResourceType resourceType, int amount)
    {
        resourceTypeAmountDictionary[resourceType] += amount;

        EventBus.Global.Publish(new OnResourceUpdated(resourceType, resourceTypeAmountDictionary[resourceType]));
    }

    public bool CanAfford(ResourceAmount resourceAmount)
    {
        return resourceTypeAmountDictionary[resourceAmount.ResourceType] >= resourceAmount.Amount;
    }

    public bool CanAfford(ResourceAmount[] resourceAmounts)
    {
        foreach (var resourceAmount in resourceAmounts)
        {
            if (!CanAfford(resourceAmount))
            {
                return false;
            }
        }

        return true;
    }

    public void TrySpendResource(ResourceAmount resourceAmount)
    {
        if (!CanAfford(resourceAmount))
        {
            return;
        }

        resourceTypeAmountDictionary[resourceAmount.ResourceType] -= resourceAmount.Amount;
        EventBus.Global.Publish(new OnResourceUpdated(resourceAmount.ResourceType, resourceTypeAmountDictionary[resourceAmount.ResourceType]));
    }

    public void TrySpendResource(ResourceAmount[] resourceAmounts)
    {
        if (!CanAfford(resourceAmounts))
        {
            return;
        }

        foreach (var resourceAmount in resourceAmounts)
        {
            resourceTypeAmountDictionary[resourceAmount.ResourceType] -= resourceAmount.Amount;
            EventBus.Global.Publish(new OnResourceUpdated(resourceAmount.ResourceType, resourceTypeAmountDictionary[resourceAmount.ResourceType]));
        }
    }
}
