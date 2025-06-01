using Unity.Entities;
using UnityEngine;

public class TurretRotatorAuthoring : MonoBehaviour
{
	public GameObject turretHead;

	public class Baker : Baker<TurretRotatorAuthoring>
	{
		public override void Bake(TurretRotatorAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new TurretRotator
			                     {
				                     TurretHeadEntity = GetEntity(authoring.turretHead, TransformUsageFlags.Dynamic)
			                     });
		}
	}
}

public struct TurretRotator : IComponentData
{
	public Entity TurretHeadEntity;
}