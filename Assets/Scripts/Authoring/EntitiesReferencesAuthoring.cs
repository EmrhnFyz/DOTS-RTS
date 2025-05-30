using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
	public GameObject bulletPrefabGameObject;
	public GameObject zombiePrefabGameObject;
	public GameObject shootLightPrefabGameObject;

	public class Baker : Baker<EntitiesReferencesAuthoring>
	{
		public override void Bake(EntitiesReferencesAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new EntitiesReferences
			                     {
				                     BulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic),
				                     ZombiePrefabEntity = GetEntity(authoring.zombiePrefabGameObject, TransformUsageFlags.Dynamic),
				                     ShootLightPrefabEntity = GetEntity(authoring.shootLightPrefabGameObject, TransformUsageFlags.Dynamic)
			                     });
		}
	}
}

public struct EntitiesReferences : IComponentData
{
	public Entity BulletPrefabEntity;
	public Entity ZombiePrefabEntity;
	public Entity ShootLightPrefabEntity;
}