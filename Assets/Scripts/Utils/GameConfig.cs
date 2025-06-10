using Unity.Physics;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
	public const int DEFAULT_LAYER = 0;
	public const int UNIT_LAYER = 6; // Layer for units
	public const int AMMUNITION_LAYER = 7; // Layer for ammunition
	public const int BUILDING_LAYER = 8; // Layer for buildings
	public const int PATHFINDING_WALLS = 9; // Layer for pathfinding walls
	public const int PATHFINDING_HEAVY = 10; // Layer for pathfinding heavy
	public const int FOW_LAYER = 12;

	public const float REACH_TARGET_DISTANCE_SQ = 0.01f; // Squared distance to consider the target reached
	public const float TARGET_PROXIMITY_TRESHOLD = 3f; // Distance threshold to consider a target nearby
	public const float MAX_RAY_DISTANCE = 9999f;
	public const float VISUAL_UNDER_FOW_COOLDOWN = 0.2f; // Cooldown for visual under fog of war update

	public const float HORDE_START_SPAWNING_SOON_TIME = 15f; // Time before a horde starts spawning soon
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

	public static readonly CollisionFilter BuildingPlacementCollisionFilter = new()
	                                                                          {
		                                                                          BelongsTo = ~0u,
		                                                                          CollidesWith = (1u << BUILDING_LAYER) | (1u << DEFAULT_LAYER),
		                                                                          GroupIndex = 0
	                                                                          };

	public static readonly CollisionFilter PathfindingWallCollisionFilter = new()
	                                                                        {
		                                                                        BelongsTo = ~0u,
		                                                                        CollidesWith = 1u << PATHFINDING_WALLS,
		                                                                        GroupIndex = 0
	                                                                        };

	public static readonly CollisionFilter PathfindingHeavyCollisionFilter = new()
	                                                                         {
		                                                                         BelongsTo = ~0u,
		                                                                         CollidesWith = 1u << PATHFINDING_HEAVY,
		                                                                         GroupIndex = 0
	                                                                         };

	public static readonly CollisionFilter MeleeAttackCollisionFilter = new()
	                                                                    {
		                                                                    BelongsTo = ~0u,
		                                                                    CollidesWith = (1u << UNIT_LAYER) | (1u << BUILDING_LAYER),
		                                                                    GroupIndex = 0
	                                                                    };

	public static readonly CollisionFilter FOWCollisionFilter = new()
	                                                            {
		                                                            BelongsTo = ~0u,
		                                                            CollidesWith = 1u << FOW_LAYER,
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