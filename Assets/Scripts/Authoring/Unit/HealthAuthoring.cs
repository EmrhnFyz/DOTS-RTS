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
	public float MaxHealth;
	public float CurrentHealth;
	public bool OnHealthChanged;

	public Health(float maxHealth)
	{
		MaxHealth = maxHealth;
		CurrentHealth = maxHealth;
		OnHealthChanged = true;
	}
}