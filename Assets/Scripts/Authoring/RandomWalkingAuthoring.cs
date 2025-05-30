using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class RandomWalkingAuthoring : MonoBehaviour
{
	public float3 targetPosition;
	public float3 originPosition;
	public float distanceMin;
	public float distanceMax;
	public uint randomSeed;

	public class Baker : Baker<RandomWalkingAuthoring>
	{
		public override void Bake(RandomWalkingAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new RandomWalking
			                     {
				                     TargetPosition = authoring.targetPosition,
				                     OriginPosition = authoring.originPosition,
				                     DistanceMin = authoring.distanceMin,
				                     DistanceMax = authoring.distanceMax,
				                     Rng = new Random(authoring.randomSeed)
			                     });
		}
	}
}

public struct RandomWalking : IComponentData
{
	public float3 TargetPosition;
	public float3 OriginPosition;
	public float DistanceMin;
	public float DistanceMax;
	public Random Rng; // Random number generator for this component
}