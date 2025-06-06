using Unity.Physics;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
	public const int UNIT_LAYER = 6; // Layer for units
	public const int AMMUNITION_LAYER = 7; // Layer for ammunition
	public const int BUILDING_LAYER = 8; // Layer for buildings
	public const int PATHFINDING_WALLS = 9; // Layer for pathfinding walls

	public const float REACH_TARGET_DISTANCE_SQ = 0.01f; // Squared distance to consider the target reached
	public const float TARGET_PROXIMITY_TRESHOLD = 3f; // Distance threshold to consider a target nearby
	public const float MAX_RAY_DISTANCE = 9999f;

	public static GameConfig Instance { get; private set; }
	public static Camera CachedCamera { get; private set; } // Cached camera reference

	public static readonly CollisionFilter UnitSelectionCollisionFilter = new()
	                                                                      {
		                                                                      BelongsTo = ~0u,
		                                                                      CollidesWith = 1u << UNIT_LAYER,
		                                                                      GroupIndex = 0
	                                                                      };

	/// <summary>
	///     This collision filter is used for faction related entities like units and buildings.
	/// </summary>
	public static readonly CollisionFilter FactionSelectionCollisionFilter = new()
	                                                                         {
		                                                                         BelongsTo = ~0u,
		                                                                         CollidesWith = (1u << UNIT_LAYER) | (1u << BUILDING_LAYER),
		                                                                         GroupIndex = 0
	                                                                         };

	public static readonly CollisionFilter PathfindingWallCollisionFilter = new()
	                                                                        {
		                                                                        BelongsTo = ~0u,
		                                                                        CollidesWith = 1u << PATHFINDING_WALLS,
		                                                                        GroupIndex = 0
	                                                                        };

	[SerializeField] private BuildingTypeSOEventChannelSO onActiveBuildingTypeChangedEventChannel;

	public static bool IsBuildingPlacementActive;

	private void Awake()
	{
		Instance = this;
		CachedCamera = Camera.main; // Cache the main camera reference
	}

	private void OnEnable()
	{
		onActiveBuildingTypeChangedEventChannel.RegisterListener(e => SetIsBuildingPlacementActive(e.Value));
	}

	private void OnDisable()
	{
		onActiveBuildingTypeChangedEventChannel.UnregisterListener(e => SetIsBuildingPlacementActive(e.Value));
	}

	private static void SetIsBuildingPlacementActive(BuildingTypeSO buildingTypeSO)
	{
		IsBuildingPlacementActive = buildingTypeSO.buildingType != BuildingType.None;
	}

	public static Vector3 GetCameraForward() => !CachedCamera ? Vector3.zero : CachedCamera.transform.forward;

	public UnitTypeListSO unitTypeListSO;
	public BuildingTypeListSO buildingTypeListSO;
}