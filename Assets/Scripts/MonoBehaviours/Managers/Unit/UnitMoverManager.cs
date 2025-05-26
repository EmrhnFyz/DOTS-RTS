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

	private void Awake()
	{
		_playerInputActions = new PlayerInputActions();
	}

	private void Start()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}

	private void OnEnable()
	{
		_move = _playerInputActions.Player.Move;
		_move.Enable();

		_move.performed += MoveToSelectedPosition;
	}

	private void OnDisable()
	{
		_move.Disable();
		_move.performed -= MoveToSelectedPosition;
	}

	private void MoveToSelectedPosition(InputAction.CallbackContext context)
	{
		var mouseWorldPosition = MouseWorldPosition.Instance.GetMousePosition();
		var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(_entityManager);
		var unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
		var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
		for (var i = 0; i < unitMoverArray.Length; i++)
		{
			var unitMover = unitMoverArray[i];
			unitMover.targetPosition = mouseWorldPosition;
			unitMoverArray[i] = unitMover;
		}

		entityQuery.CopyFromComponentDataArray(unitMoverArray);
	}
}