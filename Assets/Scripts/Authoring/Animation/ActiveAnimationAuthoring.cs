using Unity.Entities;
using UnityEngine;

public class ActiveAnimationAuthoring : MonoBehaviour
{
	public AnimationType nextAnimationType;

	public class Baker : Baker<ActiveAnimationAuthoring>
	{
		public override void Bake(ActiveAnimationAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ActiveAnimation
			                     {
				                     NextAnimationType = authoring.nextAnimationType
			                     });
		}
	}
}

public struct ActiveAnimation : IComponentData
{
	public AnimationType ActiveAnimationType;
	public AnimationType NextAnimationType;
	
	public int Frame;

	public float FrameTimer;
}