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
				                     speed = authoring.speed,
				                     damageAmount = authoring.damageAmount,
				                     direction = float3.zero,
				                     distanceTraveled = 0f,
				                     maxDistance = authoring.maxDistance
			                     });
		}
	}
}

public struct Bullet : IComponentData
{
	public float speed;
	public float damageAmount;
	public float distanceTraveled;
	public float maxDistance;

	public float3 direction;
}