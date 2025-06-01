using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitMoverManager : MonoBehaviour
{
	private EntityManager _entityManager;

	private PlayerInputActions _playerInputActions;

	private InputAction _move;
	private InputAction _select;

	private FormationType _currentFormationType;

	private Dictionary<FormationType, FormationBase> _formations;

	[SerializeField] private FormationTypeEventChannelSO _formationTypeEventChannel;


	private void Awake()
	{
		_playerInputActions = new PlayerInputActions();
		CreateFormations();
	}

	private void Start()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}

	private void CreateFormations()
	{
		_formations = new Dictionary<FormationType, FormationBase>
		              {
			              { FormationType.Square, FormationFactory.Create<SquareFormation>(FormationType.Square, 5) },
			              { FormationType.Circle, FormationFactory.Create<CircleFormation>(FormationType.Circle, 5) },
			              { FormationType.ArrowOutline, FormationFactory.Create<OutlineArrowFormation>(FormationType.ArrowOutline, 3) },
			              { FormationType.ArrowFilled, FormationFactory.Create<FilledArrowFormation>(FormationType.ArrowFilled, 3) }
		              };
	}

	private void OnEnable()
	{
		_move = _playerInputActions.Player.Move;
		_move.Enable();

		_move.performed += MoveToSelectedPosition;

		_formationTypeEventChannel.RegisterListener(e => OnFormationTypeChanged(e.Value));
	}

	private void OnDisable()
	{
		_move.Disable();
		_move.performed -= MoveToSelectedPosition;

		_formationTypeEventChannel.UnregisterListener(e => OnFormationTypeChanged(e.Value));
	}

	private void OnFormationTypeChanged(FormationType newFormationType)
	{
		if (_formations.TryGetValue(newFormationType, out _))
		{
			_currentFormationType = newFormationType;
		}
	}

	private void MoveToSelectedPosition(InputAction.CallbackContext context)
	{
		var mouseWorldPosition = MouseWorldPosition.Instance.GetMousePosition();

		var isAttackingSingleTarget = OnEnemySelected();

		if (!isAttackingSingleTarget)
		{
			TrySetBarracksRallyPosition();
			var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<MoveOverride>().Build(_entityManager);
			var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
			var moveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);

			if (moveOverrideArray.Length == 0)
			{
				return;
			}

			// create formation based on the current formation type
			var totalUnitCount = moveOverrideArray.Length;
			var direction = (mouseWorldPosition - (Vector3)moveOverrideArray[0].TargetPosition).normalized;
			var formation = _formations[_currentFormationType].CalculateFormationPositions(totalUnitCount, mouseWorldPosition, direction);
			for (var i = 0; i < moveOverrideArray.Length; i++)
			{
				var moveOverride = moveOverrideArray[i];
				moveOverride.TargetPosition = formation[i];
				moveOverrideArray[i] = moveOverride;
				_entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);
			}

			entityQuery.CopyFromComponentDataArray(moveOverrideArray);
		}
	}

	private bool OnEnemySelected()
	{
		var entityQuery = _entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
		var physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
		var collisionWorld = physicsWorldSingleton.CollisionWorld;
		var cameraRay = GameConfig.CachedCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

		var raycastInput = new RaycastInput
		                   {
			                   Start = cameraRay.GetPoint(0.1f),
			                   End = cameraRay.GetPoint(GameConfig.MAX_RAY_DISTANCE),
			                   Filter = GameConfig.FactionSelectionCollisionFilter
		                   };

		if (!collisionWorld.CastRay(raycastInput, out var raycastHit))
		{
			return false;
		}

		if (!_entityManager.HasComponent<Faction>(raycastHit.Entity))
		{
			return false;
		}

		var clickedUnit = _entityManager.GetComponentData<Faction>(raycastHit.Entity);

		if (clickedUnit.FactionType != Factions.Enemy)
		{
			return false;
		}

		entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<TargetOverride>().Build(_entityManager);
		var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
		var targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);

		for (var i = 0; i < targetOverrideArray.Length; i++)
		{
			var targetOverride = targetOverrideArray[i];
			targetOverride.TargetEntity = raycastHit.Entity;
			targetOverrideArray[i] = targetOverride;
			_entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
		}

		entityQuery.CopyFromComponentDataArray(targetOverrideArray);
		return true;
	}

	//Handle Barracks Rally Position
	public void TrySetBarracksRallyPosition()
	{
		var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, Barracks, LocalTransform>().Build(_entityManager);
		var barracksArray = entityQuery.ToComponentDataArray<Barracks>(Allocator.Temp);

		if (barracksArray.Length == 0)
		{
			return;
		}

		var localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
		var mouseWorldPosition = MouseWorldPosition.Instance.GetMousePosition();

		for (var i = 0; i < barracksArray.Length; i++)
		{
			var barracks = barracksArray[i];
			barracks.rallyPositionOffset = (float3)mouseWorldPosition - localTransformArray[i].Position;
			barracksArray[i] = barracks;
		}

		entityQuery.CopyFromComponentDataArray(barracksArray);
	}
}