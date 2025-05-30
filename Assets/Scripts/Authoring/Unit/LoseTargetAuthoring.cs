using Unity.Entities;
using UnityEngine;

public class LoseTargetAuthoring : MonoBehaviour
{
	public float loseTargetDistance;

	public class Baker : Baker<LoseTargetAuthoring>
	{
		public override void Bake(LoseTargetAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new LoseTarget
			                     {
				                     LoseDistance = authoring.loseTargetDistance
			                     }); // Default lose distance can be adjusted
		}
	}
}

public struct LoseTarget : IComponentData
{
	public float LoseDistance;
}