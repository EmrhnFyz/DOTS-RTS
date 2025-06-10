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
	[SerializeField] private Material ghostRedMaterial;

	private Material _activeGhostMaterial;


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

			if (!ResourceManager.Instance.CanAfford(buildingTypeSO.cost))
			{
				SetGhostMaterial(ghostRedMaterial);
				TooltipScreenSpaceUI.ShowTooltip_Static(
					buildingTypeSO.NameString + "\n" +
					ResourceAmount.GetString(buildingTypeSO.cost) + "\n" +
					"<color=#ff0000>Cannot afford resource cost!</color>", 0.5f);
			}
			else
			{
				SetGhostMaterial(ghostMaterial);
				TooltipScreenSpaceUI.ShowTooltip_Static(
					buildingTypeSO.NameString + "\n" +
					ResourceAmount.GetString(buildingTypeSO.cost), 0.5f);
			}

			if (!CanPlaceBuilding())
			{
				// Cannot place building here
				SetGhostMaterial(ghostRedMaterial);
				TooltipScreenSpaceUI.ShowTooltip_Static(
					buildingTypeSO.NameString + "\n" +
					ResourceAmount.GetString(buildingTypeSO.cost) + "\n" +
					"<color=#ff0000>" + "Cant Place Building Here" + "</color>", 0.5f);
			}
			else
			{
				TooltipScreenSpaceUI.ShowTooltip_Static(
					buildingTypeSO.NameString + "\n" +
					ResourceAmount.GetString(buildingTypeSO.cost), .05f);
			}
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
		if (buildingTypeSO.buildingType == BuildingType.None)
		{
			return;
		}

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

		if (buildingTypeSO.buildingType == BuildingType.None)
		{
			return;
		}

		TooltipScreenSpaceUI.ShowTooltip_Static(
			buildingTypeSO.NameString + "\n" +
			ResourceAmount.GetString(buildingTypeSO.cost), .05f);

		if (!ResourceManager.Instance.CanAfford(buildingTypeSO.cost))
		{
			return;
		}

		if (CanPlaceBuilding())
		{
			SetGhostMaterial(ghostMaterial);

			var mouseWorldPosition = MouseWorldPosition.Instance.GetMousePosition();


			ResourceManager.Instance.TrySpendResource(buildingTypeSO.cost);
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
			var entityReferences = entityQuery.GetSingleton<EntitiesReferences>();


			var spawnedConstructionVisualEntity = entityManager.Instantiate(buildingTypeSO.GetConstructionVisualPrefabEntity(entityReferences));
			entityManager.SetComponentData(spawnedConstructionVisualEntity, LocalTransform.FromPosition(mouseWorldPosition + new Vector3(0, buildingTypeSO.constructionYOffset, 0)));

			var spawnedConstructionEntity = entityManager.Instantiate(entityReferences.ConstructionPrefabEntity);
			entityManager.SetComponentData(spawnedConstructionEntity, LocalTransform.FromPosition(mouseWorldPosition));

			entityManager.SetComponentData(spawnedConstructionEntity, new Construction
			                                                          {
				                                                          BuildingType = buildingTypeSO.buildingType,
				                                                          ConstructionTimer = 0f,
				                                                          ConstructionTimeMax = buildingTypeSO.constructionTimerMax,
				                                                          FinalPrefabEntity = buildingTypeSO.GetBuildingPrefabEntity(entityReferences),
				                                                          VisualEntity = spawnedConstructionVisualEntity,
				                                                          StartPosition = mouseWorldPosition + new Vector3(0, buildingTypeSO.constructionYOffset, 0),
				                                                          EndPosition = mouseWorldPosition
			                                                          });
		}
	}

	private void SetGhostMaterial(Material ghostMaterial)
	{
		if (_activeGhostMaterial == ghostMaterial)
		{
			// Already set this material
			return;
		}

		_activeGhostMaterial = ghostMaterial;

		foreach (var meshRenderer in ghostTransform.GetComponentsInChildren<MeshRenderer>())
		{
			meshRenderer.material = ghostMaterial;
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

		if (buildingTypeSO is ResourceHarvesterTypeSO resourceHarvesterTypeSO)
		{
			var hasValidResource = false;
			var mouseWorldPosition = MouseWorldPosition.Instance.GetMousePosition();
			if (collisionWorld.OverlapSphere(
				    mouseWorldPosition,
				    resourceHarvesterTypeSO.harvestDistance,
				    ref distanceHitList,
				    GameConfig.BuildingPlacementCollisionFilter
			    ))
			{
				// hit something within harvest distance
				foreach (var distanceHit in distanceHitList)
				{
					if (entityManager.HasComponent<ResourceTypeSOHolder>(distanceHit.Entity))
					{
						var resourceTypeSOHolder = entityManager.GetComponentData<ResourceTypeSOHolder>(distanceHit.Entity);
						if (resourceTypeSOHolder.ResourceType == resourceHarvesterTypeSO.resourceType)
						{
							// nearby valid resource
							hasValidResource = true;
							break;
						}
					}
				}
			}

			if (!hasValidResource)
			{
				return false;
			}
		}

		return true;
	}
}