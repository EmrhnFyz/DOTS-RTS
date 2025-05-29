using Unity.Entities;
using UnityEngine;

public class TargetAuthoring : MonoBehaviour
{
	public class Baker : Baker<TargetAuthoring>
	{
		public override void Bake(TargetAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Target());
		}
	}
}

public struct Target : IComponentData
{
	public Entity TargetEntity;
}