using UnityEngine;

public class RagdollCache : MonoBehaviour
{
	private Rigidbody[] _rigidbodies;

	private void Awake()
	{
		_rigidbodies = GetComponentsInChildren<Rigidbody>();
	}

	public void ResetRagdoll()
	{
		foreach (var rb in _rigidbodies)
		{
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.transform.localRotation = Quaternion.identity;
		}
	}
}