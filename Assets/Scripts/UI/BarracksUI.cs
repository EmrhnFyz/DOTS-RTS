using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityEventKit;

[Serializable]
public struct ImageQueueItem
{
	public UnitType UnitType;
	public Sprite Image;
}

public class BarracksUI : MonoBehaviour
{
	[SerializeField] private BoolEventChannelSO onBarracksSelected;

	[SerializeField] private List<ImageQueueItem> _unitQueueItemList = new();

	[SerializeField] private RectTransform unitQueueContainer;

	[SerializeField] private Button soldierButton;
	[SerializeField] private Button scoutButton;

	[SerializeField] private Image progressBar;
	[SerializeField] private Image unitQueueImageTemplate;

	[SerializeField] private TMP_Text progressText;


	private readonly StringBuilder _stringBuilder = new();

	private EntityManager _entityManager;
	private EntityQuery _entityQuery;

	private Entity _barracksEntity;

	private readonly Queue<Image> _unitQueueImages = new();

	private readonly Queue<Image> _activeUnitQueue = new(30);
	private readonly Dictionary<UnitType, Sprite> _unitImageDictionary = new();

	private void Awake()
	{
		soldierButton.onClick.AddListener(() =>
		{
			AddUnitToSpawnQueue(UnitType.Soldier);
		});

		scoutButton.onClick.AddListener(() =>
		{
			AddUnitToSpawnQueue(UnitType.Scout);
		});
	}

	private void PrewarmUnitQueueImages()
	{
		for (var i = 0; i < 30; i++)
		{
			var image = Instantiate(unitQueueImageTemplate, unitQueueContainer);
			image.gameObject.SetActive(false);
			_unitQueueImages.Enqueue(image);
		}
	}

	private void OnEnable()
	{
		onBarracksSelected.RegisterListener(e => OnBarracksSelected(e.Value));
	}

	private void OnDisable()
	{
		onBarracksSelected.UnregisterListener(e => OnBarracksSelected(e.Value));
	}

	private void OnDestroy()
	{
		onBarracksSelected.UnregisterListener(e => OnBarracksSelected(e.Value));
	}

	private void Start()
	{
		Hide();
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		PrewarmUnitQueueImages();

		foreach (var unitQueueItem in _unitQueueItemList)
		{
			if (!_unitImageDictionary.ContainsKey(unitQueueItem.UnitType))
			{
				_unitImageDictionary.Add(unitQueueItem.UnitType, unitQueueItem.Image);
			}
		}
	}

	private void OnBarracksSelected(bool isSelected)
	{
		if (isSelected)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Show()
	{
		gameObject.SetActive(true);
		progressBar.fillAmount = 0f;
		progressText.text = "Ready!";
	}

	private void Hide()
	{
		gameObject.SetActive(false);
	}

	private void AddUnitToSpawnQueue(UnitType unitType)
	{
		if (TrySetSelectedBarracks())
		{
			return;
		}

		var spawnUnitTypeDynamicBuffer = _entityManager.GetBuffer<SpawnUnitTypeBuffer>(_barracksEntity);

		PlaceUnitQueueImage(unitType);
		spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
		                               {
			                               UnitType = unitType
		                               });
	}

	private bool TrySetSelectedBarracks()
	{
		if (_barracksEntity == Entity.Null)
		{
			_entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, Barracks>().Build(_entityManager);
			if (_entityQuery.IsEmpty)
			{
				return true;
			}

			using (var barracksArray = _entityQuery.ToEntityArray(Allocator.Temp))
			{
				if (barracksArray.Length > 0)
				{
					_barracksEntity = barracksArray[0];
				}
				else
				{
					return true;
				}
			}
		}

		return false;
	}

	private void PlaceUnitQueueImage(UnitType unitType)
	{
		// if  queue is empty, add new image to the queue and use it
		if (_unitQueueImages.Count == 0)
		{
			var newImage = Instantiate(unitQueueImageTemplate, unitQueueContainer);
			newImage.gameObject.SetActive(false);
			_unitQueueImages.Enqueue(newImage);
		}

		var image = _unitQueueImages.Dequeue();
		image.gameObject.SetActive(true);
		image.sprite = _unitImageDictionary[unitType];

		_activeUnitQueue.Enqueue(image);
	}

	private void RemoveUnitFromQueueVisual()
	{
		if (_activeUnitQueue.Count > 0)
		{
			var image = _activeUnitQueue.Dequeue();
			image.gameObject.SetActive(false);
			_unitQueueImages.Enqueue(image);
		}
	}

	private void Update()
	{
		if (_activeUnitQueue.Count == 0)
		{
			return;
		}

		UpdateProgressBar();

		HandleSpawnQueueVisual();
	}

	private void HandleSpawnQueueVisual()
	{
		if (TrySetSelectedBarracks())
		{
			return;
		}

		var spawnUnitTypeDynamicBuffer = _entityManager.GetBuffer<SpawnUnitTypeBuffer>(_barracksEntity, true);

		if (spawnUnitTypeDynamicBuffer.Length < _activeUnitQueue.Count)
		{
			// if there are more units in the queue than images, remove the last image
			RemoveUnitFromQueueVisual();
		}
	}

	private void UpdateProgressBar()
	{
		if (!gameObject.activeSelf || _barracksEntity == Entity.Null)
		{
			progressBar.fillAmount = 0f;
			progressText.text = "Ready!";
			return;
		}

		var barracksData = _entityManager.GetComponentData<Barracks>(_barracksEntity);

		if (barracksData.UnitTypeToSpawn == UnitType.None)
		{
			progressBar.fillAmount = 0f;
			progressText.text = "Ready!";
			return;
		}

		_stringBuilder.Clear();

		if (barracksData.Progress >= barracksData.SpawnTime)
		{
			progressBar.fillAmount = 1f;
			progressText.text = "Ready!";
		}
		else
		{
			_stringBuilder.Append(barracksData.Progress.ToString("F1"));
			_stringBuilder.Append(" / ");
			_stringBuilder.Append(barracksData.SpawnTime.ToString("F1"));
			_stringBuilder.Append(" secs");

			progressBar.fillAmount = barracksData.Progress / barracksData.SpawnTime;
			progressText.text = _stringBuilder.ToString();
		}
	}
}