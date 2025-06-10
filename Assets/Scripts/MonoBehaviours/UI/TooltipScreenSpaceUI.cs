using System;
using TMPro;
using UnityEngine;

public class TooltipScreenSpaceUI : MonoBehaviour
{
	public static TooltipScreenSpaceUI Instance { get; private set; }

	[SerializeField] private RectTransform canvasRectTransform;

	private RectTransform _backgroundRectTransform;
	private TMP_Text _text;
	private RectTransform _rectTransform;


	private Func<string> _getTooltipTextFunc;
	private float? _showTimer;


	private void Awake()
	{
		Instance = this;

		_backgroundRectTransform = transform.Find("background").GetComponent<RectTransform>();
		_text = transform.Find("text").GetComponent<TextMeshProUGUI>();
		_rectTransform = transform.GetComponent<RectTransform>();


		HideTooltip();
	}

	private void SetText(string tooltipText)
	{
		_text.SetText(tooltipText);
		_text.ForceMeshUpdate();

		var textSize = _text.GetRenderedValues(false);
		var paddingSize = new Vector2(8, 8);

		_backgroundRectTransform.sizeDelta = textSize + paddingSize;
	}

	private void Update()
	{
		SetText(_getTooltipTextFunc());

		PositionTooltip();

		if (_showTimer != null)
		{
			_showTimer -= Time.deltaTime;
			if (_showTimer <= 0)
			{
				HideTooltip();
			}
		}
	}

	private void PositionTooltip()
	{
		Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

		if (anchoredPosition.x + _backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
		{
			// Tooltip left screen on right side
			anchoredPosition.x = canvasRectTransform.rect.width - _backgroundRectTransform.rect.width;
		}

		if (anchoredPosition.y + _backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
		{
			// Tooltip left screen on top side
			anchoredPosition.y = canvasRectTransform.rect.height - _backgroundRectTransform.rect.height;
		}

		_rectTransform.anchoredPosition = anchoredPosition;
	}

	private void ShowTooltip(string tooltipText, float? showTimer)
	{
		ShowTooltip(() => tooltipText, showTimer);
	}

	private void ShowTooltip(Func<string> getTooltipTextFunc, float? showTimer)
	{
		_getTooltipTextFunc = getTooltipTextFunc;
		_showTimer = showTimer;
		gameObject.SetActive(true);
		SetText(getTooltipTextFunc());
		PositionTooltip();
	}

	private void HideTooltip()
	{
		gameObject.SetActive(false);
	}

	public static void ShowTooltip_Static(string tooltipText, float? showTimer)
	{
		Instance.ShowTooltip(tooltipText, showTimer);
	}

	public static void ShowTooltip_Static(Func<string> getTooltipTextFunc, float? showTimer)
	{
		Instance.ShowTooltip(getTooltipTextFunc, showTimer);
	}

	public static void HideTooltip_Static()
	{
		Instance.HideTooltip();
	}
}