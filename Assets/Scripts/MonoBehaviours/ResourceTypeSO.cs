using UnityEngine;


public enum ResourceType
{
    None,
    Iron,
    Gold,
    Oil
}

[CreateAssetMenu(fileName = "ResourceTypeSO", menuName = "ResourceTypes/ResourceTypeSO")]
public class ResourceTypeSO : ScriptableObject
{
    public ResourceType ResourceType;
    public Sprite Sprite;

}
