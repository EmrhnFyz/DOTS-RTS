using System.Collections.Generic;
using Unity.Mathematics;

public class CircleFormation : FormationBase
{
	public CircleFormation(float spacing = 1f) : base(spacing)
	{
	}

	public override List<float3> CalculateFormationPositions(int unitCount, float3 targetPosition, float3 forward)
	{
		positions.Clear();

		if (unitCount <= 0)
		{
			return positions;
		}

		positions.Add(targetPosition);

		// center
		var remaining = unitCount - 1;
		var ring = 1;

		// fill rings until we placed all units
		while (remaining > 0)
		{
			var radius = ring * spacing;
			var circumference = 2f * math.PI * radius;

			// how many units we can fit at roughly  spacing along the ring
			var capacity = math.max(1, (int)math.floor(circumference / spacing));
			var toPlace = math.min(capacity, remaining);
			var angleStep = 2f * math.PI / toPlace;

			for (var i = 0; i < toPlace; i++)
			{
				var angle = i * angleStep;
				var x = math.cos(angle) * radius;
				var z = math.sin(angle) * radius;
				positions.Add(targetPosition + new float3(x, 0, z));
			}

			remaining -= toPlace;
			ring++;
		}

		return positions;
	}
}