using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
	public float speed;
	public float damageAmount;
	public float maxDistance = 30f; // Maximum travel distance before auto-destroy

	public class Baker : Baker<BulletAuthoring>
	{
		public override void Bake(BulletAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Bullet
			                     {
				                     Speed = authoring.speed,
				                     DamageAmount = authoring.damageAmount,
				                     Direction = float3.zero,
				                     DistanceTraveled = 0f,
				                     MaxDistance = authoring.maxDistance
			                     });
		}
	}
}

public struct Bullet : IComponentData
{
	public float Speed;
	public float DamageAmount;
	public float DistanceTraveled;
	public float MaxDistance;

	public float3 Direction;
}