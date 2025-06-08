using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
	public GameObject bulletPrefabGameObject;
	public GameObject zombiePrefabGameObject;
	public GameObject shootLightPrefabGameObject;
	public GameObject scoutPrefabGameObject;
	public GameObject soldierPrefabGameObject;
	public GameObject barracksPrefabGameObject;
	public GameObject turretPrefabGameObject;
	public GameObject goldHarvesterPrefabGameObject;
	public GameObject ironHarvesterPrefabGameObject;
	public GameObject oilHarvesterPrefabGameObject;


	public class Baker : Baker<EntitiesReferencesAuthoring>
	{
		public override void Bake(EntitiesReferencesAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new EntitiesReferences
			{
				BulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic),
				ZombiePrefabEntity = GetEntity(authoring.zombiePrefabGameObject, TransformUsageFlags.Dynamic),
				ShootLightPrefabEntity = GetEntity(authoring.shootLightPrefabGameObject, TransformUsageFlags.Dynamic),
				ScoutPrefabEntity = GetEntity(authoring.scoutPrefabGameObject, TransformUsageFlags.Dynamic),
				SoldierPrefabEntity = GetEntity(authoring.soldierPrefabGameObject, TransformUsageFlags.Dynamic),
				BarracksPrefabEntity = GetEntity(authoring.barracksPrefabGameObject, TransformUsageFlags.Dynamic),
				TurretPrefabEntity = GetEntity(authoring.turretPrefabGameObject, TransformUsageFlags.Dynamic),
				GoldHarvesterPrefabEntity = GetEntity(authoring.goldHarvesterPrefabGameObject, TransformUsageFlags.Dynamic),
				IronHarvesterPrefabEntity = GetEntity(authoring.ironHarvesterPrefabGameObject, TransformUsageFlags.Dynamic),
				OilHarvesterPrefabEntity = GetEntity(authoring.oilHarvesterPrefabGameObject, TransformUsageFlags.Dynamic)
			});
		}
	}
}

public struct EntitiesReferences : IComponentData
{
	public Entity BulletPrefabEntity;
	public Entity ZombiePrefabEntity;
	public Entity ShootLightPrefabEntity;
	public Entity ScoutPrefabEntity;
	public Entity SoldierPrefabEntity;
	public Entity BarracksPrefabEntity;
	public Entity TurretPrefabEntity;
	public Entity GoldHarvesterPrefabEntity;
	public Entity IronHarvesterPrefabEntity;
	public Entity OilHarvesterPrefabEntity;
}