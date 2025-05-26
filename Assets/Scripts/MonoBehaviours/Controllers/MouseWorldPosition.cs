using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWorldPosition : MonoBehaviour
{
	public static MouseWorldPosition Instance;
	private Camera _mainCamera;
	private Plane _plane;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_mainCamera = Camera.main;
		_plane = new Plane(Vector3.up, Vector3.zero);
	}

	public Vector3 GetMousePosition()
	{
		var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

		return _plane.Raycast(ray, out var hit) ? ray.GetPoint(hit) : Vector3.zero;
	}
}