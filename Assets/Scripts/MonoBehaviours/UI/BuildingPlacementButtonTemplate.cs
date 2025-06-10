using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingPlacementButtonTemplate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private GameObject selectedIndicator;
	[SerializeField] private Image iconImage;

	public Button button;

	public BuildingTypeSO buildingTypeSO;

	public void SetupButton(BuildingTypeSO buildingTypeSO)
	{
		SetSelected(false);
		iconImage.sprite = buildingTypeSO.iconSprite;
		this.buildingTypeSO = buildingTypeSO;
	}

	public void SetSelected(bool isSelected)
	{
		if (!selectedIndicator)
		{
			return;
		}

		selectedIndicator.SetActive(isSelected);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		TooltipScreenSpaceUI.ShowTooltip_Static(
			buildingTypeSO.NameString + "\n" +
			ResourceAmount.GetString(buildingTypeSO.cost), 99f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TooltipScreenSpaceUI.HideTooltip_Static();
	}
}