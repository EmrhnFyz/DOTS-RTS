using Unity.Entities;
using UnityEngine;

public class HQAuthoring : MonoBehaviour
{
	public class Baker : Baker<HQAuthoring>
	{
		public override void Bake(HQAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new HQ());
		}
	}
}

public struct HQ : IComponentData
{
}