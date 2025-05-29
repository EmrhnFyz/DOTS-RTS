using Unity.Entities;
using UnityEngine;

public class ShootAuthoring : MonoBehaviour
{
	public float cooldown = 0.2f;
	public float damageAmount;

	public class Baker : Baker<ShootAuthoring>
	{
		public override void Bake(ShootAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Shoot
			                     {
				                     cooldown = authoring.cooldown,
				                     damageAmount = authoring.damageAmount
			                     });
		}
	}
}

public struct Shoot : IComponentData
{
	public float cooldown;
	public float timer;
	public float damageAmount;
}