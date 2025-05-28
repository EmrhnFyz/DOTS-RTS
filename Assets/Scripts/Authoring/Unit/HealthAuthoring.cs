using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
	public float maxHealth;

	public class Baker : Baker<HealthAuthoring>
	{
		public override void Bake(HealthAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Health(authoring.maxHealth));
		}
	}
}

public struct Health : IComponentData
{
	public float maxHealth;
	public float currentHealth;

	public Health(float maxHealth)
	{
		this.maxHealth = maxHealth;
		currentHealth = maxHealth;
	}
}