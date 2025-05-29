using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerAuthoring : MonoBehaviour
{
	public float cooldown;

	public class Baker : Baker<ZombieSpawnerAuthoring>
	{
		public override void Bake(ZombieSpawnerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ZombieSpawner
			                     {
				                     Cooldown = authoring.cooldown
			                     });
		}
	}
}

public struct ZombieSpawner : IComponentData
{
	public float Timer;
	public float Cooldown;
}