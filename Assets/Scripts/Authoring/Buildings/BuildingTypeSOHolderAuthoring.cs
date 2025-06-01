using Unity.Entities;
using UnityEngine;

public class BuildingTypeSOHolderAuthoring : MonoBehaviour
{
	public BuildingType buildingType; // Reference to the BuildingTypeSO ScriptableObject

	public class Baker : Baker<BuildingTypeSOHolderAuthoring>
	{
		public override void Bake(BuildingTypeSOHolderAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new BuildingTypSOHolder
			                     {
				                     BuildingType = authoring.buildingType // Assign the ScriptableObject reference to the component
			                     });
		}
	}
}

public struct BuildingTypSOHolder : IComponentData
{
	public BuildingType BuildingType; // Reference to the BuildingTypeSO ScriptableObject
}