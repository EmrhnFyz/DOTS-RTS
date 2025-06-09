using Unity.Burst;
using Unity.Entities;

internal partial struct MainMenuSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<MainMenuSceneTag>();
	}
}