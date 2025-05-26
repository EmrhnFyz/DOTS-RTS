using Unity.Collections;
using Unity.Mathematics;

// Filled-triangle arrow formation: row 0 is the tip (1 unit),
//row 1 has 3 units, row 2 has 5 units, etc., until we place all units.
public class FilledArrowFormation : FormationBase
{
	public FilledArrowFormation(float spacing = 1f) : base(spacing)
	{
	}

	public override NativeArray<float3> CalculateFormationPositions(int unitCount, float3 targetPosition, float3 forward)
	{
		if (unitCount <= 0)
		{
			return new NativeArray<float3>(0, Allocator.Temp);
		}

		var localOffsets = new NativeArray<float3>(unitCount, Allocator.Temp);
		var placed = 0;
		var row = 0;
		var counter = 0;

		// fill rows until we’ve placed all units
		while (placed < unitCount)
		{
			// number of slots in this row: 2*row + 1
			var slots = 2 * row + 1;

			// for each column in this row, from -row to +row
			for (var c = -row; c <= row && placed < unitCount; c++)
			{
				var x = c * spacing;
				var z = -row * spacing; // row 0 at z=0, row 1 at z=-spacing, etc.
				localOffsets[counter] = new float3(x, 0, z);
				placed++;
				counter++;
			}

			row++;
		}

		// rotate so “forward” -> local +Z
		var rot = math.normalize(
			quaternion.LookRotationSafe(forward, new float3(0, 1, 0)));

		// apply rotation + translate to world
		var worldPositions = new NativeArray<float3>(unitCount, Allocator.Temp);
		for (var index = 0; index < localOffsets.Length; index++)
		{
			var lo = localOffsets[index];
			worldPositions[index] = targetPosition + math.mul(rot, lo);
		}

		return worldPositions;
	}
}