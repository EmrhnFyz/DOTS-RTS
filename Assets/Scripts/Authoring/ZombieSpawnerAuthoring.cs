using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerAuthoring : MonoBehaviour
{
	public float cooldown;
	public float randomWalkingDistanceMin;
	public float randomWalkingDistanceMax;

	public class Baker : Baker<ZombieSpawnerAuthoring>
	{
		public override void Bake(ZombieSpawnerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ZombieSpawner
			                     {
				                     Cooldown = authoring.cooldown,
				                     RandomWalkingDistanceMax = authoring.randomWalkingDistanceMax,
				                     RandomWalkingDistanceMin = authoring.randomWalkingDistanceMin
			                     });
		}
	}
}

public struct ZombieSpawner : IComponentData
{
	public float Timer;
	public float Cooldown;
	public float RandomWalkingDistanceMin;
	public float RandomWalkingDistanceMax;
}