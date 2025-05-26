using Unity.Collections;
using Unity.Mathematics;

public abstract class FormationBase
{
	protected float spacing;
	protected FormationBase(float spacing = 1f) => this.spacing = spacing;


	public abstract NativeArray<float3> CalculateFormationPositions(int unitCount, float3 targetPosition, float3 forward);
}