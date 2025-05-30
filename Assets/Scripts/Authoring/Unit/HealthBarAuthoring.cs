using Unity.Entities;
using UnityEngine;

public class HealthBarAuthoring : MonoBehaviour
{
	public GameObject barHolderGameObject; // Reference to the health bar holder GameObject
	public GameObject healthGameObject; // Reference to the health GameObject

	public class Baker : Baker<HealthBarAuthoring>
	{
		public override void Bake(HealthBarAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new HealthBar
			                     {
				                     barHolderEntity = GetEntity(authoring.barHolderGameObject, TransformUsageFlags.NonUniformScale),
				                     healthEntity = GetEntity(authoring.healthGameObject, TransformUsageFlags.Dynamic)
			                     });
		}
	}
}

public struct HealthBar : IComponentData
{
	public Entity barHolderEntity; // Entity representing the health bar holder
	public Entity healthEntity;
}