using System.Collections.Generic;
using UnityEngine;
using UnityEventKit;

public class BuildingPlacementManagerUI : MonoBehaviour
{
	[SerializeField] private BuildingTypeSOEventChannelSO onActiveBuildingTypeChangedEventChannel;
	[SerializeField] private VoidEventChannelSO onBuildingPlacementCancelledEventChannel;

	[SerializeField] private RectTransform buildingPlacementContainer;
	[SerializeField] private BuildingPlacementButtonTemplate buildingPlacementButtonTemplate;
	[SerializeField] private BuildingTypeListSO buildingTypeListSO;

	private readonly List<BuildingPlacementButtonTemplate> _buildingPlacementButtons = new();

	private void Awake()
	{
		foreach (var buildingTypeSO in buildingTypeListSO.buildingTypeSOList)
		{
			if (!buildingTypeSO.isPlaceable)
			{
				continue;
			}

			var placedButton = Instantiate(buildingPlacementButtonTemplate, buildingPlacementContainer);
			placedButton.SetupButton(buildingTypeSO);
			placedButton.gameObject.SetActive(true);
			_buildingPlacementButtons.Add(placedButton);
			placedButton.button.onClick.AddListener(() => OnButtonClicked(placedButton));
		}

		_buildingPlacementButtons[0].SetSelected(true);
	}

	private void OnEnable()
	{
		onBuildingPlacementCancelledEventChannel.RegisterListener(e => SelectNoneButton());
	}

	private void OnDisable()
	{
		onBuildingPlacementCancelledEventChannel.UnregisterListener(e => SelectNoneButton());
	}

	private void SelectNoneButton()
	{
		foreach (var button in _buildingPlacementButtons)
		{
			if (button.buildingTypeSO != buildingTypeListSO.none)
			{
				continue;
			}

			OnButtonClicked(button);
			return;
		}
	}

	private void DeselectAllButtons()
	{
		foreach (var button in _buildingPlacementButtons)
		{
			button.SetSelected(false);
		}
	}

	private void OnButtonClicked(BuildingPlacementButtonTemplate clickedButton)
	{
		DeselectAllButtons();

		clickedButton.SetSelected(true);

		onActiveBuildingTypeChangedEventChannel.Raise(new ValueEvent<BuildingTypeSO>(clickedButton.buildingTypeSO));
	}
}