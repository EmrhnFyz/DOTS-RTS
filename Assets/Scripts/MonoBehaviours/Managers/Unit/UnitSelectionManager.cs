using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEventKit;

public class UnitSelectionManager : MonoBehaviour
{
	[SerializeField] private Vector2EventChannelSO _onSelectionBoxStarted;
	[SerializeField] private Vector2EventChannelSO _onSelectionBoxEnded;

	private Camera _mainCamera;
	private PlayerInputActions _playerInputActions;

	private InputAction _onLeftButtonPressed;
	private InputAction _onLeftButtonReleased;

	private EntityManager _entityManager;

	private static Vector2 _selectionStartPosition;
	private Vector2 _selectionEndPosition;

	private readonly float _minSelectionBoxSize = 80;
	private const float MAX_RAY_DISTANCE = 9999f;

	private void Awake()
	{
		_playerInputActions = new PlayerInputActions();
	}

	private void Start()
	{
		_mainCamera = Camera.main;
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}

	private void OnEnable()
	{
		_onLeftButtonPressed = _playerInputActions.Player.SelectStart;
		_onLeftButtonReleased = _playerInputActions.Player.SelectionEnd;

		_onLeftButtonPressed.Enable();
		_onLeftButtonReleased.Enable();

		_onLeftButtonPressed.performed += OnLeftMouseButtonPressed;
		_onLeftButtonReleased.performed += OnLeftMouseButtonReleased;
	}

	private void OnDisable()
	{
		_onLeftButtonPressed.Disable();
		_onLeftButtonReleased.Disable();
		_onLeftButtonPressed.performed -= OnLeftMouseButtonPressed;
		_onLeftButtonReleased.performed -= OnLeftMouseButtonReleased;
	}


	private void OnLeftMouseButtonPressed(InputAction.CallbackContext context)
	{
		_selectionStartPosition = Mouse.current.position.ReadValue();
		_onSelectionBoxStarted.Raise(new ValueEvent<Vector2>(_selectionStartPosition));
	}

	private void OnLeftMouseButtonReleased(InputAction.CallbackContext context)
	{
		_selectionEndPosition = Mouse.current.position.ReadValue();
		_onSelectionBoxEnded.Raise(new ValueEvent<Vector2>(_selectionEndPosition));
		var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(_entityManager);

		DeselectAllEntities(entityQuery);
		var selectionAreaRect = GetSelectionBoxRect();
		var selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
		if (selectionAreaSize > _minSelectionBoxSize)
		{
			entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(_entityManager);

			SelectEntitiesInRectBox(entityQuery, selectionAreaRect);
		}
		else
		{
			entityQuery = _entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
			SelectSingleEntity(entityQuery);
		}
	}

	private void SelectSingleEntity(EntityQuery entityQuery)
	{
		var physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
		var collisionWorld = physicsWorldSingleton.CollisionWorld;
		var cameraRay = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

		var raycastInput = new RaycastInput
		                   {
			                   Start = cameraRay.GetPoint(0.1f),
			                   End = cameraRay.GetPoint(MAX_RAY_DISTANCE),
			                   Filter = new CollisionFilter
			                            {
				                            BelongsTo = ~0u,
				                            CollidesWith = 1u << GameConfig.UNIT_LAYER,
				                            GroupIndex = 0
			                            }
		                   };
		if (collisionWorld.CastRay(raycastInput, out var raycastHit))
		{
			_entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
			var selected = _entityManager.GetComponentData<Selected>(raycastHit.Entity);
			selected.onSelected = true;
			_entityManager.SetComponentData(raycastHit.Entity, selected);
		}
	}

	private void SelectEntitiesInRectBox(EntityQuery entityQuery, Rect selectionAreaRect)
	{
		var localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
		var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
		for (var i = 0; i < localTransformArray.Length; i++)
		{
			var unitLocalTransform = localTransformArray[i];
			var unitScreenPosition = _mainCamera.WorldToScreenPoint(unitLocalTransform.Position);

			if (selectionAreaRect.Contains(unitScreenPosition))
			{
				_entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
				var selected = _entityManager.GetComponentData<Selected>(entityArray[i]);
				selected.onSelected = true;
				_entityManager.SetComponentData(entityArray[i], selected);
			}
		}
	}

	private void DeselectAllEntities(EntityQuery entityQuery)
	{
		var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
		var selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
		for (var i = 0; i < entityArray.Length; i++)
		{
			_entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
			var selected = selectedArray[i];
			selected.onDeselected = true;
			_entityManager.SetComponentData(entityArray[i], selected);
		}
	}

	public static Rect GetSelectionBoxRect()
	{
		var currentMousePosition = Mouse.current.position.ReadValue();

		Vector2 lowerLeftCorner = new(Mathf.Min(_selectionStartPosition.x, currentMousePosition.x),
		                              Mathf.Min(_selectionStartPosition.y, currentMousePosition.y));

		Vector2 upperRightCorner = new(Mathf.Max(_selectionStartPosition.x, currentMousePosition.x),
		                               Mathf.Max(_selectionStartPosition.y, currentMousePosition.y));

		return new Rect(lowerLeftCorner.x, lowerLeftCorner.y, upperRightCorner.x - lowerLeftCorner.x, upperRightCorner.y - lowerLeftCorner.y);
	}
}