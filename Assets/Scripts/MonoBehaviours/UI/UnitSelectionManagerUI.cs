using UnityEngine;
using UnityEventKit;

public class UnitSelectionManagerUI : MonoBehaviour
{
	[SerializeField] private RectTransform _selectionBoxRectTransform;
	[SerializeField] private Canvas _canvas;

	[SerializeField] private Vector2EventChannelSO _onSelectionBoxStarted;
	[SerializeField] private Vector2EventChannelSO _onSelectionBoxEnded;

	private void Awake()
	{
		if (_selectionBoxRectTransform == null)
		{
			Debug.LogError("Selection Box RectTransform is not assigned.");
			return;
		}

		_selectionBoxRectTransform.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		_onSelectionBoxStarted.RegisterListener(e => OnSelectionBoxStarted(e.Value));
		_onSelectionBoxEnded.RegisterListener(e => OnSelectionBoxEnded(e.Value));
	}

	private void OnDisable()
	{
		_onSelectionBoxStarted.UnregisterListener(e => OnSelectionBoxStarted(e.Value));
		_onSelectionBoxEnded.UnregisterListener(e => OnSelectionBoxEnded(e.Value));
	}

	private void Update()
	{
		if (_selectionBoxRectTransform.gameObject.activeSelf)
		{
			UpdateVisual();
		}
	}

	private void OnSelectionBoxStarted(Vector2 startPosition)
	{
		_selectionBoxRectTransform.gameObject.SetActive(true);
		UpdateVisual();
	}

	private void OnSelectionBoxEnded(Vector2 endPosition)
	{
		var startPosition = _selectionBoxRectTransform.anchoredPosition;
		_selectionBoxRectTransform.gameObject.SetActive(false);
	}

	private void UpdateVisual()
	{
		var selectionAreaRect = UnitSelectionManager.GetSelectionBoxRect();
		var canvasScale = _canvas.transform.localScale.x;

		_selectionBoxRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
		_selectionBoxRectTransform.sizeDelta = new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
	}
}