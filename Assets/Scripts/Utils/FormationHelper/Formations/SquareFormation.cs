using System.Collections.Generic;
using Unity.Mathematics;

public class SquareFormation : FormationBase
{
	public SquareFormation(float spacing = 1f) : base(spacing)
	{
	}

	public override List<float3> CalculateFormationPositions(int unitCount, float3 targetPosition, float3 forward)
	{
		positions.Clear();
		var cols = (int)math.ceil(math.sqrt(unitCount));
		var rows = (int)math.ceil(unitCount / (float)cols);

		var halfW = (cols - 1) * spacing * 0.5f; // half width of the formation
		var halfD = (rows - 1) * spacing * 0.5f; // half depth of the formation

		for (var i = 0; i < unitCount; i++)
		{
			var row = i / cols;
			var col = i % cols;

			var xOffset = col * spacing - halfW;
			var zOffset = row * spacing - halfD;

			var position = targetPosition + forward * zOffset + math.cross(forward, new float3(0, 1, 0)) * xOffset;
			positions.Add(position);
		}

		return positions;
	}
}