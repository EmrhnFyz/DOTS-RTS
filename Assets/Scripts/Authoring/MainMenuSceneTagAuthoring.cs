using Unity.Entities;
using UnityEngine;

public class MainMenuSceneTagAuthoring : MonoBehaviour
{
	public class Baker : Baker<GameSceneTagAuthoring>
	{
		public override void Bake(GameSceneTagAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new MainMenuSceneTag());
		}
	}
}

public struct MainMenuSceneTag : IComponentData
{
	// This struct is intentionally left empty. It serves as a marker component to identify the main menu scene.
}