using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacementButtonTemplate : MonoBehaviour
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
		selectedIndicator.SetActive(isSelected);
	}
}