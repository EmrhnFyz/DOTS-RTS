using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEventKit;

public class DOTSEventManager : MonoBehaviour
{
	public static DOTSEventManager Instance { get; private set; }

	[SerializeField] private VoidEventChannelSO _onHQDeathEventChannel;

	public readonly struct OnDeathEvent : IEvent
	{
		public readonly Entity Entity;

		public OnDeathEvent(Entity entity) => Entity = entity;
	}

	private void Awake()
	{
		Instance = this;
	}

	public void TriggerOnHQDeath()
	{
		_onHQDeathEventChannel.Raise(new VoidEvent());
	}

	public void TriggerOnDeath(NativeList<Entity> entityNativeList)
	{
		foreach (var entity in entityNativeList)
		{
			EventBus.Global.Publish(new OnDeathEvent(entity));
		}
	}
}