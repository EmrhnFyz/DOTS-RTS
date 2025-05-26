using System.Collections.Generic;
using Unity.Mathematics;

public abstract class FormationBase
{
	protected float spacing;
	protected List<float3> positions = new(50);
	protected List<float3> localOffsets = new(50);
	protected FormationBase(float spacing = 1f) => this.spacing = spacing;


	public abstract List<float3> CalculateFormationPositions(int unitCount, float3 targetPosition, float3 forward);
}