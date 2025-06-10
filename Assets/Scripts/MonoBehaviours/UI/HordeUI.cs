using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEventKit;

public class HordeUI : MonoBehaviour
{
	[SerializeField] private RectTransform arrowRectTransform;
	[SerializeField] private GameObject container;
	private Entity _spawningSoonEntity;

	private SubscriptionToken _onHordeStartSpawningSoonToken;
	private SubscriptionToken _onHordeStartSpawningToken;

	private void OnEnable()
	{
		_onHordeStartSpawningSoonToken = EventBus.Global.Subscribe<DOTSEventManager.OnHordeStartSpawningSoonEvent>(e => OnHordeStartSpawningSoon(e.HordeEntity));
		_onHordeStartSpawningToken = EventBus.Global.Subscribe<DOTSEventManager.OnHordeStartSpawningEvent>(e => OnHordeStartSpawning(e.HordeEntity));
	}

	private void Update()
	{
		UpdateArrowVisual();
	}

	private void UpdateArrowVisual()
	{
		if (_spawningSoonEntity == Entity.Null)
		{
			arrowRectTransform.gameObject.SetActive(false);
			return;
		}

		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		var spawningSoonLocalTransform = entityManager.GetComponentData<LocalTransform>(_spawningSoonEntity);

		var cameraRay = GameConfig.CachedCamera.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

		if (Physics.Raycast(cameraRay, out var raycasthit, 999))
		{
			var dir = ((Vector3)spawningSoonLocalTransform.Position - raycasthit.point).normalized;

			arrowRectTransform.eulerAngles = new Vector3(0, 0, -Quaternion.LookRotation(dir).eulerAngles.y);
		}
	}


	private void OnHordeStartSpawning(Entity startSpawningEntity)
	{
		container.SetActive(false);

		if (_spawningSoonEntity == startSpawningEntity)
		{
			_spawningSoonEntity = Entity.Null; // Reset the spawning soon entity
		}
	}

	private void OnHordeStartSpawningSoon(Entity spawningSoonEntity)
	{
		_spawningSoonEntity = spawningSoonEntity;
		container.SetActive(true);
		UpdateArrowVisual();
	}
}