using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
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
		var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(_entityManager);
		var unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

		if (unitMoverArray.Length == 0)
		{
			return;
		}

		// create formation based on the current formation type
		var totalUnitCount = unitMoverArray.Length;
		var direction = (mouseWorldPosition - (Vector3)unitMoverArray[0].TargetPosition).normalized;
		var formation = _formations[_currentFormationType].CalculateFormationPositions(totalUnitCount, mouseWorldPosition, direction);
		for (var i = 0; i < unitMoverArray.Length; i++)
		{
			var unitMover = unitMoverArray[i];
			unitMover.TargetPosition = formation[i];
			unitMoverArray[i] = unitMover;
		}

		entityQuery.CopyFromComponentDataArray(unitMoverArray);
	}
}