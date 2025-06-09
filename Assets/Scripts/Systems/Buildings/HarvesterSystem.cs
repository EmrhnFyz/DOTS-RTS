using Unity.Burst;
using Unity.Entities;

internal partial struct HarvesterSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<GameSceneTag>();
	}

	public void OnUpdate(ref SystemState state)
	{
		foreach (var harvester in SystemAPI.Query<RefRW<Harvester>>())
		{
			harvester.ValueRW.HarvestTimer -= SystemAPI.Time.DeltaTime;
			if (harvester.ValueRO.HarvestTimer <= 0f)
			{
				harvester.ValueRW.HarvestTimer = harvester.ValueRO.Cooldown;

				ResourceManager.Instance.AddResource(harvester.ValueRO.ResourceType, harvester.ValueRO.HarvestRate);
			}
		}
	}
}