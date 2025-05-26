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
				                     selectionMark = GetEntity(authoring._selectionMark, TransformUsageFlags.Dynamic),
				                     showScale = authoring.showScale
			                     });

			SetComponentEnabled<Selected>(entity, false);
		}
	}
}

public struct Selected : IComponentData, IEnableableComponent
{
	public Entity selectionMark;
	public float showScale;

	public bool onSelected;
	public bool onDeselected;
}