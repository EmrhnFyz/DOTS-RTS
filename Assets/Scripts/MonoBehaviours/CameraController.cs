using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
	private PlayerInputActions _playerInputActions;

	private InputAction _moveForward;
	private InputAction _moveBackward;
	private InputAction _moveLeft;
	private InputAction _moveRight;
	private InputAction _rotateLeft;
	private InputAction _rotateRight;
	private InputAction _scrollWheel;

	[SerializeField] private float moveSpeed = 30f;
	[SerializeField] private float rotationSpeed = 100f;
	[SerializeField] private float zoomSpeed = 10f;
	[SerializeField] private float fieldOfViewMin = 20f;
	[SerializeField] private float fieldOfViewMax = 80f;

	[SerializeField] private CinemachineCamera cinemachineCamera;

	private float targetFieldOfView;

	private void Awake()
	{
		_playerInputActions = new PlayerInputActions();
		targetFieldOfView = cinemachineCamera.Lens.FieldOfView;
	}

	private void OnEnable()
	{
		_moveForward = _playerInputActions.Camera.MoveForward;
		_moveBackward = _playerInputActions.Camera.MoveBack;
		_moveLeft = _playerInputActions.Camera.MoveLeft;
		_moveRight = _playerInputActions.Camera.MoveRight;

		_rotateLeft = _playerInputActions.Camera.RotateLeft;
		_rotateRight = _playerInputActions.Camera.RotateRight;

		_scrollWheel = _playerInputActions.Camera.MouseScroll;

		_moveForward.Enable();
		_moveBackward.Enable();
		_moveLeft.Enable();
		_moveRight.Enable();

		_rotateLeft.Enable();
		_rotateRight.Enable();

		_scrollWheel.Enable();
	}

	private void OnDisable()
	{
		_moveForward.Disable();
		_moveBackward.Disable();
		_moveLeft.Disable();
		_moveRight.Disable();

		_rotateLeft.Disable();
		_rotateRight.Disable();

		_scrollWheel.Disable();
	}

	private void Update()
	{
		var moveDirection = Vector3.zero;
		var cachedCameraForward = GameConfig.CachedCamera.transform.forward;
		var cachedCameraRight = GameConfig.CachedCamera.transform.right;

		if (_moveForward.IsPressed())
		{
			moveDirection += Vector3.forward;
		}

		if (_moveBackward.IsPressed())
		{
			moveDirection += Vector3.back;
		}

		if (_moveLeft.IsPressed())
		{
			moveDirection += Vector3.left;
		}

		if (_moveRight.IsPressed())
		{
			moveDirection += Vector3.right;
		}

		moveDirection = cachedCameraForward * moveDirection.z + cachedCameraRight * moveDirection.x;
		moveDirection.y = 0f;
		moveDirection.Normalize();

		transform.position += moveDirection * moveSpeed * Time.deltaTime;


		if (_rotateLeft.IsPressed())
		{
			transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
		}

		if (_rotateRight.IsPressed())
		{
			transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
		}

		if (_scrollWheel.ReadValue<Vector2>().y < 0)
		{
			targetFieldOfView += 4f;
		}
		else if (_scrollWheel.ReadValue<Vector2>().y > 0)
		{
			targetFieldOfView -= 4f;
		}

		targetFieldOfView = Mathf.Clamp(targetFieldOfView, fieldOfViewMin, fieldOfViewMax);
		cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(cinemachineCamera.Lens.FieldOfView, targetFieldOfView, zoomSpeed * Time.deltaTime);
	}
}