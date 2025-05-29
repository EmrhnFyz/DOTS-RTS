using Unity.Entities;
using UnityEngine;

public class UnitSelectionAuthoring : MonoBehaviour
{
	[SerializeField]
	private GameObject _selectionMark;

	public float showScale;

	private class SelectedAuthoringBaker : Baker<UnitSelectionAuthoring>
	{
		public override void Bake(UnitSelectionAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Selected
			                     {
				                     SelectionMark = GetEntity(authoring._selectionMark, TransformUsageFlags.Dynamic),
				                     ShowScale = authoring.showScale
			                     });

			SetComponentEnabled<Selected>(entity, false);
		}
	}
}

public struct Selected : IComponentData, IEnableableComponent
{
	public Entity SelectionMark;
	public float ShowScale;

	public bool OnSelected;
	public bool OnDeselected;
}