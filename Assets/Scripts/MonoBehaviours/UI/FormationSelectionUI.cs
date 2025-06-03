using TMPro;
using UnityEngine;
using UnityEventKit;

public class FormationSelectionUI : MonoBehaviour
{
	public FormationType formationType;
	[SerializeField] private TMP_Text _formationNameText;
	[SerializeField] private FormationTypeEventChannelSO _formationTypeEventChannel;

	public void Start()
	{
		OnFormationSelected(0);
	}

	public void OnFormationSelected(int selectedType)
	{
		formationType = (FormationType)selectedType;
		UpdateFormationName();

		_formationTypeEventChannel.Raise(new ValueEvent<FormationType>(formationType));
	}

	private void UpdateFormationName()
	{
		switch (formationType)
		{
			case FormationType.Square:
				_formationNameText.text = "Square";
				break;
			case FormationType.Circle:
				_formationNameText.text = "Circle";
				break;
			case FormationType.ArrowOutline:
				_formationNameText.text = "Arrow Outline";
				break;
			case FormationType.ArrowFilled:
				_formationNameText.text = "Arrow Filled";
				break;
			default:
				_formationNameText.text = "Unknown Formation";
				break;
		}
	}
}