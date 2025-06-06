using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FlowFieldFollowerAuthoring : MonoBehaviour
{
	public class Baker : Baker<FlowFieldFollowerAuthoring>
	{
		public override void Bake(FlowFieldFollowerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FlowFieldFollower());
			SetComponentEnabled<FlowFieldFollower>(entity, false);
		}
	}
}

public struct FlowFieldFollower : IComponentData, IEnableableComponent
{
	public float3 TargetPosition;
	public float3 LastMoveVector;
	public int GridIndex;
}