using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceTypeListSO", menuName = "ResourceTypes/ResourceTypeListSO")]
public class ResourceTypeListSO : ScriptableObject
{
    public List<ResourceTypeSO> resourceTypeSOList;
}
