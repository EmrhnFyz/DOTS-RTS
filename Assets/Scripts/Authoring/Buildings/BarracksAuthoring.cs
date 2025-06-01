using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BarracksAuthoring : MonoBehaviour
{
	public float spawnTime = 10f;

	public class Baker : Baker<BarracksAuthoring>
	{
		public override void Bake(BarracksAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Barracks
			                     {
				                     SpawnTime = authoring.spawnTime,
				                     rallyPositionOffset = new float3(10, 0, 0) // Default offset for rally point
			                     });

			var spawnUnitTypeDynamicBuffer = AddBuffer<SpawnUnitTypeBuffer>(entity);

			spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
			                               {
				                               UnitType = UnitType.Soldier
			                               });
			spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
			                               {
				                               UnitType = UnitType.Soldier
			                               });
			spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
			                               {
				                               UnitType = UnitType.Scout
			                               });
			spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
			                               {
				                               UnitType = UnitType.Soldier
			                               });
			spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
			                               {
				                               UnitType = UnitType.Scout
			                               });
		}
	}
}

public struct Barracks : IComponentData
{
	public UnitType UnitTypeToSpawn; // Type of unit to spawn, can be used for default spawning

	public float Progress; // Current progress of the barracks
	public float SpawnTime; // Time it takes to spawn a unit

	public float3 rallyPositionOffset; // Offset from the barracks position to the rally point
}

[InternalBufferCapacity(16)]
public struct SpawnUnitTypeBuffer : IBufferElementData
{
	public UnitType UnitType; // Type of unit to spawn
}