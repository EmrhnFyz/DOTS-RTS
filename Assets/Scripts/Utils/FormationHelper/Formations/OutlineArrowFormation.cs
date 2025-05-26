using System.Collections.Generic;
using Unity.Mathematics;

// V‐shaped “arrow” formation: one tip in front, others in expanding V‐rows behind.
public class OutlineArrowFormation : FormationBase
{
	public OutlineArrowFormation(float spacing = 1f) : base(spacing)
	{
	}

	public override List<float3> CalculateFormationPositions(int unitCount, float3 targetPosition, float3 forward)
	{
		localOffsets.Clear();
		if (unitCount <= 0)
		{
			return localOffsets;
		}

		// tip of the arrow
		localOffsets.Add(float3.zero);
		var placed = 1;
		var depth = 1;

		// build V rows behind the tip 
		while (placed < unitCount)
		{
			// left wing
			if (placed < unitCount)
			{
				localOffsets.Add(new float3(-depth * spacing, 0, -depth * spacing));
				placed++;
			}

			// right wing
			if (placed < unitCount)
			{
				localOffsets.Add(new float3(depth * spacing, 0, -depth * spacing));
				placed++;
			}

			depth++;
		}

		//Rotate the entire V so “forward” maps to local +Z
		var rotation = math.normalize(quaternion.LookRotationSafe(forward, new float3(0, 1, 0)));

		for (var i = 0; i < localOffsets.Count; i++)
		{
			var worldOffset = math.mul(rotation, localOffsets[i]);
			localOffsets[i] = targetPosition + worldOffset;
		}

		return localOffsets;
	}
}