using UnityEngine;
using UnityEventKit;

public class DOTSEventManager : MonoBehaviour
{
	public static DOTSEventManager Instance { get; private set; }

	[SerializeField] private VoidEventChannelSO _onHQDeathEventChannel;

	private void Awake()
	{
		Instance = this;
	}

	public void TriggerOnHQDeath()
	{
		_onHQDeathEventChannel.Raise(new VoidEvent());
	}
}