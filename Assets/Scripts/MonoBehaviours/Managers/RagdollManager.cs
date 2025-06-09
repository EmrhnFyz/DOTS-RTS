using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEventKit;

public class RagdollManager : MonoBehaviour
{
	private SubscriptionToken _onDeathEventToken;
	[SerializeField] private UnitTypeListSO unitTypeListSO;

	private readonly Dictionary<UnitType, Queue<RagdollCache>> _ragdollPrefabs = new();

	protected void Start()
	{
		PrewarmRagdolls(15);
	}

	protected void OnEnable()
	{
		_onDeathEventToken = EventBus.Global.Subscribe<DOTSEventManager.OnDeathEvent>(e => OnDeath(e.Entity));
	}

	protected void OnDisable()
	{
		_onDeathEventToken.Dispose();
	}

	private void PrewarmRagdolls(int poolSize)
	{
		foreach (var unitTypeSO in unitTypeListSO.unitTypeSOList)
		{
			if (unitTypeSO.unitType == UnitType.None)
			{
				continue;
			}

			if (!_ragdollPrefabs.ContainsKey(unitTypeSO.unitType))
			{
				_ragdollPrefabs[unitTypeSO.unitType] = new Queue<RagdollCache>();
			}

			for (var i = 0; i < poolSize; i++)
			{
				var ragdollInstance = Instantiate(unitTypeSO.ragdollPrefab);
				ragdollInstance.gameObject.SetActive(false);
				if (ragdollInstance.TryGetComponent<RagdollCache>(out var ragdollCache))
				{
					_ragdollPrefabs[unitTypeSO.unitType].Enqueue(ragdollCache);
				}
			}
		}
	}

	private void OnDeath(Entity entity)
	{
		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		if (entityManager.HasComponent<UnitTypeHolder>(entity))
		{
			var localTransform = entityManager.GetComponentData<LocalTransform>(entity);
			var unitTypeHolder = entityManager.GetComponentData<UnitTypeHolder>(entity);
			var unitTypeSO = unitTypeListSO.GetUnitTypeSO(unitTypeHolder.UnitType);
			if (!unitTypeSO || !_ragdollPrefabs.TryGetValue(unitTypeSO.unitType, out var ragdollQueue))
			{
				return;
			}

			if (ragdollQueue.Count == 0)
			{
				// If the queue is empty, create a new instance and add it to the queue
				var ragdollInstance = Instantiate(unitTypeSO.ragdollPrefab);
				ragdollInstance.gameObject.SetActive(false);
				if (ragdollInstance.TryGetComponent<RagdollCache>(out var ragdollCache))
				{
					ragdollQueue.Enqueue(ragdollCache);
				}
				else
				{
					return;
				}
			}

			var ragdoll = ragdollQueue.Dequeue();
			ragdoll.ResetRagdoll();
			ragdoll.transform.position = localTransform.Position + new float3(0, 0.5f, 0); // Adjust height to avoid clipping into the ground
			ragdoll.transform.rotation = localTransform.Rotation;
			ragdoll.gameObject.SetActive(true);

			DespawnRagdoll(ragdoll, unitTypeSO.unitType).Forget();
		}
	}


	private async UniTask DespawnRagdoll(RagdollCache ragdollCache, UnitType unitType)
	{
		await UniTask.Delay(5000); // Wait for 5 seconds before despawning
		if (ragdollCache && ragdollCache.gameObject.activeSelf)
		{
			ragdollCache.gameObject.SetActive(false);
			if (_ragdollPrefabs.TryGetValue(unitType, out var ragdollQueue))
			{
				ragdollQueue.Enqueue(ragdollCache);
			}
		}
	}
}