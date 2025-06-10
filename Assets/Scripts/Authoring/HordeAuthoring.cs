using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class HordeAuthoring : MonoBehaviour
{
	public float startTimer;
	public float spawnTimerMax;
	public int zombieAmountToSpawn;
	public float spawnAreaWidth;
	public float spawnAreaHeight;

	public GameObject minimapIconGameObject;

	public class Baker : Baker<HordeAuthoring>
	{
		public override void Bake(HordeAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			var minimapIconEntity = GetEntity(authoring.minimapIconGameObject, TransformUsageFlags.Dynamic);
			AddComponent(entity, new Horde
			                     {
				                     StartTimer = authoring.startTimer,
				                     SpawnCooldown = authoring.spawnTimerMax,
				                     ZombieAmountToSpawn = authoring.zombieAmountToSpawn,
				                     SpawnAreaHeight = authoring.spawnAreaHeight,
				                     SpawnAreaWidth = authoring.spawnAreaWidth,
				                     Random = new Random((uint)entity.Index), // Ensure a unique seed for each entity,
				                     IsSetup = false,
				                     MinimapIconEntity = minimapIconEntity
			                     });
		}
	}
}

public struct Horde : IComponentData
{
	public float StartTimer;
	public float SpawnTimer;
	public float SpawnCooldown;

	public float SpawnAreaWidth;
	public float SpawnAreaHeight;

	public Random Random;

	public bool IsSetup;
	public bool OnStartSpawningSoon;
	public bool OnStartSpawning;

	public int ZombieAmountToSpawn;

	public Entity MinimapIconEntity;
}