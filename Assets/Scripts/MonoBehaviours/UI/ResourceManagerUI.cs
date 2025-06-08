using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEventKit;

public class ResourceManagerUI : MonoBehaviour
{
    [SerializeField] private ResourceUIObject resourceTemplate;
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private ResourceTypeListSO resourceTypeListSO;

    private Dictionary<ResourceType, ResourceUIObject> resourceTypeToUIObjectDictionary;

    private SubscriptionToken onResourceUpdatedSubscriptionToken;
    private void Awake()
    {
        resourceTypeToUIObjectDictionary = new Dictionary<ResourceType, ResourceUIObject>();
        Setup();
    }
    private void OnEnable()
    {
        onResourceUpdatedSubscriptionToken = EventBus.Global.Subscribe<ResourceManager.OnResourceUpdated>(UpdateResourceAmount);
    }

    private void OnDisable()
    {
        onResourceUpdatedSubscriptionToken.Dispose();
    }

    private void Setup()
    {
        foreach (var resourceTypeSO in resourceTypeListSO.resourceTypeSOList)
        {
            ResourceUIObject resourceTransform = Instantiate(resourceTemplate, resourceContainer);
            resourceTransform.gameObject.SetActive(true);
            resourceTransform.Setup(resourceTypeSO);
            resourceTypeToUIObjectDictionary.Add(resourceTypeSO.ResourceType, resourceTransform);
        }
    }


    public void UpdateResourceAmount(ResourceManager.OnResourceUpdated resourceData)
    {
        resourceTypeToUIObjectDictionary[resourceData.ResourceType].UpdateResourceAmount(resourceData.Amount);
    }
}
