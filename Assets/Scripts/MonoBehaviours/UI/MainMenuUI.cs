using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private Button _playButton;
	[SerializeField] private Button _quitButton;


	private void Awake()
	{
		_playButton.onClick.AddListener(OnPlayButtonClicked);
		_quitButton.onClick.AddListener(OnQuitButtonClicked);
	}

	private void OnPlayButtonClicked()
	{
		SceneManager.LoadScene(1);
	}


	private void OnQuitButtonClicked()
	{
		Application.Quit();

#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#endif
	}
}