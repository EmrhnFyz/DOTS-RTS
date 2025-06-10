using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerTopBarUI : MonoBehaviour
{
	[SerializeField] private TMP_Text zombieCountText;
	[SerializeField] private TMP_Text zombieSpawnerCountText;

	private float timer;
	private const float UpdateInterval = .5f;

	// Update is called once per frame
	private void Update()
	{
		timer += Time.deltaTime;
		if (timer >= UpdateInterval)
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			UpdateZombieCount(entityManager);
			UpdateZombieSpawnerCount(entityManager);
			timer = 0f;
		}
	}

	private void UpdateZombieSpawnerCount(EntityManager entityManager)
	{
		var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Unit, Enemy>().Build(entityManager);

		zombieCountText.text = entityQuery.CalculateEntityCount().ToString();

		entityQuery.Dispose();
	}

	private void UpdateZombieCount(EntityManager entityManager)
	{
		var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<ZombieSpawner>().Build(entityManager);
		zombieSpawnerCountText.text = entityQuery.CalculateEntityCount().ToString();

		entityQuery.Dispose();
	}
}