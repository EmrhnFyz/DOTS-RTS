using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEventKit;
using Material = UnityEngine.Material;

public class BuildingPlacementManager : MonoBehaviour
{
	[SerializeField] private BuildingTypeSOEventChannelSO onActiveBuildingTypeChangedEventChannel;
	[SerializeField] private VoidEventChannelSO onBuildingPlacementCancelledEventChannel;
	[SerializeField] private BuildingTypeSO buildingTypeSO;

	private PlayerInputActions _playerInputActions;

	private InputAction _buildingPlacementAction;
	private InputAction _buildingPlacementCancelAction;

	private Transform ghostTransform;
	[SerializeField] private Material ghostMaterial;

	private void Awake()
	{
		_playerInputActions = new PlayerInputActions();
	}

	private void OnEnable()
	{
		_buildingPlacementAction = _playerInputActions.Player.BuildingPlacement;
		_buildingPlacementCancelAction = _playerInputActions.Player.CancelBuildingPlacement;

		_buildingPlacementAction.Enable();
		_buildingPlacementCancelAction.Enable();
		_buildingPlacementAction.performed += TryPlaceBuilding;
		_buildingPlacementCancelAction.performed += CancelBuildingPlacement;
		onActiveBuildingTypeChangedEventChannel.RegisterListener(e => SetActiveBuildingTypeSO(e.Value));
	}

	private void OnDisable()
	{
		_buildingPlacementAction.performed -= TryPlaceBuilding;
		_buildingPlacementCancelAction.performed -= CancelBuildingPlacement;
		_buildingPlacementAction.Disable();
		onActiveBuildingTypeChangedEventChannel.UnregisterListener(e => SetActiveBuildingTypeSO(e.Value));
	}

	private void Update()
	{
		if (ghostTransform)
		{
			ghostTransform.position = MouseWorldPosition.Instance.GetMousePosition();
		}
	}

	private void SetActiveBuildingTypeSO(BuildingTypeSO activeBuildingTypeSO)
	{
		buildingTypeSO = activeBuildingTypeSO;

		if (ghostTransform)
		{
			Destroy(ghostTransform.gameObject);
		}

		if (buildingTypeSO.buildingType == BuildingType.None)
		{
			return;
		}

		ghostTransform = Instantiate(buildingTypeSO.visualPrefab);
		foreach (var meshRenderer in ghostTransform.GetComponentsInChildren<MeshRenderer>())
		{
			var materialCount = meshRenderer.materials.Length;
			meshRenderer.material = ghostMaterial;
		}
	}

	private void CancelBuildingPlacement(InputAction.CallbackContext context)
	{
		onBuildingPlacementCancelledEventChannel.Raise(new VoidEvent());
	}

	private void TryPlaceBuilding(InputAction.CallbackContext context)
	{
		HandleBuildingPlacementActionAsync().Forget();
	}

	private async UniTaskVoid HandleBuildingPlacementActionAsync()
	{
		await UniTask.NextFrame(); // Wait one frame to let UI update its state

		if (EventSystem.current.IsPointerOverGameObject())
		{
			return;
		}

		if (CanPlaceBuilding())
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
			var entityReferences = entityQuery.GetSingleton<EntitiesReferences>();

			var spawnedBuildingEntity = entityManager.Instantiate(buildingTypeSO.GetBuildingPrefabEntity(entityReferences));
			entityManager.SetComponentData(spawnedBuildingEntity, LocalTransform.FromPosition(MouseWorldPosition.Instance.GetMousePosition()));
		}
	}

	private bool CanPlaceBuilding()
	{
		if (!buildingTypeSO || !buildingTypeSO.isPlaceable || buildingTypeSO.buildingType == BuildingType.None)
		{
			return false;
		}

		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		var entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
		var collisionWorld = entityQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

		var boxCollider = buildingTypeSO.buildingCollider;
		var distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
		if (collisionWorld.OverlapBox(MouseWorldPosition.Instance.GetMousePosition(), Quaternion.identity, boxCollider.size * 0.5f, ref distanceHitList, GameConfig.FactionSelectionCollisionFilter))
		{
			return false;
		}

		return true;
	}
}