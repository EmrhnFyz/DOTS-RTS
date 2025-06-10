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

	public readonly struct OnHordeStartSpawningSoonEvent : IEvent
	{
		public readonly Entity HordeEntity;

		public OnHordeStartSpawningSoonEvent(Entity hordeEntity) => HordeEntity = hordeEntity;
	}

	public readonly struct OnHordeStartSpawningEvent : IEvent
	{
		public readonly Entity HordeEntity;

		public OnHordeStartSpawningEvent(Entity hordeEntity = default) => HordeEntity = hordeEntity;
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

	public void TriggerOnHordeStartSpawningSoon(NativeList<Entity> hordeEntities)
	{
		foreach (var hordeEntity in hordeEntities)
		{
			EventBus.Global.Publish(new OnHordeStartSpawningSoonEvent(hordeEntity));
		}
	}

	public void TriggerOnHordeStartSpawning(NativeList<Entity> hordeEntities)
	{
		foreach (var hordeEntity in hordeEntities)
		{
			EventBus.Global.Publish(new OnHordeStartSpawningEvent(hordeEntity));
		}
	}
}