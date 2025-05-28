using Unity.Entities;
using UnityEngine;

public class ShootAuthoring : MonoBehaviour
{
	public float cooldown = 0.2f;
	public float damage;

	public class Baker : Baker<ShootAuthoring>
	{
		public override void Bake(ShootAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Shoot
			                     {
				                     cooldown = authoring.cooldown,
				                     damage = authoring.damage
			                     });
		}
	}
}

public struct Shoot : IComponentData
{
	public float cooldown;
	public float timer;
	public float damage;
}