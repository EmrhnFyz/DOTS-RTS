using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootAuthoring : MonoBehaviour
{
	public float cooldown = 0.2f;
	public float damageAmount;
	public GameObject nuzzle;

	public class Baker : Baker<ShootAuthoring>
	{
		public override void Bake(ShootAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Shoot
			                     {
				                     Cooldown = authoring.cooldown,
				                     DamageAmount = authoring.damageAmount,
				                     NuzzleEntity = GetEntity(authoring.nuzzle, TransformUsageFlags.Dynamic)
			                     });
		}
	}
}

public struct Shoot : IComponentData
{
	public float Cooldown;
	public float Timer;
	public float DamageAmount;

	public Entity NuzzleEntity;

	public OnShootEvent OnShoot;

	public struct OnShootEvent
	{
		public bool IsTriggered;
		public float3 ShootFromPosition;
	}
}