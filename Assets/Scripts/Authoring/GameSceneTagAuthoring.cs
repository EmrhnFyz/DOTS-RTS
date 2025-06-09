using Unity.Entities;
using UnityEngine;

public class GameSceneTagAuthoring : MonoBehaviour
{
	public class Baker : Baker<GameSceneTagAuthoring>
	{
		public override void Bake(GameSceneTagAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new GameSceneTag());
		}
	}
}

public struct GameSceneTag : IComponentData
{
	// This struct is intentionally left empty. It serves as a marker component to identify the game scene.
}