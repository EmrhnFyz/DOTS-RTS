using UnityEngine;
using UnityEventKit;

public class GameOverUI : MonoBehaviour
{
	[SerializeField] private VoidEventChannelSO _onHQDeathEventChannel;

	private void Start()
	{
		Hide();
	}

	private void OnEnable()
	{
		_onHQDeathEventChannel.RegisterListener(e => Show());
	}

	private void OnDisable()
	{
		_onHQDeathEventChannel.UnregisterListener(e => Show());
	}

	private void Show()
	{
		gameObject.SetActive(true);
		Time.timeScale = 0f; // Pause the game
	}

	private void Hide()
	{
		gameObject.SetActive(false);
	}
}