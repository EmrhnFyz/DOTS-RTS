using Unity.Collections;
using Unity.Mathematics;

// V‐shaped “arrow” formation: one tip in front, others in expanding V‐rows behind.
public class OutlineArrowFormation : FormationBase
{
	public OutlineArrowFormation(float spacing = 1f) : base(spacing)
	{
	}

	public override NativeArray<float3> CalculateFormationPositions(int unitCount, float3 targetPosition, float3 forward)
	{
		if (unitCount <= 0)
		{
			return new NativeArray<float3>(0, Allocator.Temp);
		}


		// tip of the arrow
		var localOffsets = new NativeArray<float3>(unitCount, Allocator.Temp);
		localOffsets[0] = float3.zero;
		var placed = 1;
		var depth = 1;

		var counter = 0;
		// build V rows behind the tip 
		while (placed < unitCount)
		{
			// left wing
			if (placed < unitCount)
			{
				localOffsets[counter] = new float3(-depth * spacing, 0, -depth * spacing);
				placed++;
				counter++;
			}

			// right wing
			if (placed < unitCount)
			{
				localOffsets[counter] = new float3(depth * spacing, 0, -depth * spacing);
				placed++;
				counter++;
			}

			depth++;
		}

		//Rotate the entire V so “forward” maps to local +Z
		var rotation = math.normalize(quaternion.LookRotationSafe(forward, new float3(0, 1, 0)));

		for (var i = 0; i < localOffsets.Length; i++)
		{
			var worldOffset = math.mul(rotation, localOffsets[i]);
			localOffsets[i] = targetPosition + worldOffset;
		}

		return localOffsets;
	}
}