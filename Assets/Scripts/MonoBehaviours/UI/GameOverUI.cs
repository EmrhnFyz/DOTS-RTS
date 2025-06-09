using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEventKit;

public class GameOverUI : MonoBehaviour
{
	[SerializeField] private VoidEventChannelSO _onHQDeathEventChannel;
	[SerializeField] private Button _mainMenuButton;

	private void Start()
	{
		Hide();
		_mainMenuButton.onClick.AddListener(() =>
		{
			Time.timeScale = 1f; // Resume the game
			SceneManager.LoadScene(0);
		});
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