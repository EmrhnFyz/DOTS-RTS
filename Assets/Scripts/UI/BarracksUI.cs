using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityEventKit;

public class BarracksUI : MonoBehaviour
{
	[SerializeField] private BoolEventChannelSO onBarracksSelected;

	[SerializeField] private Button soldierButton;

	[SerializeField] private Image progressBar;
	[SerializeField] private TMP_Text progressText;

	private readonly StringBuilder _stringBuilder = new();

	private EntityManager _entityManager;
	private EntityQuery _entityQuery;

	private Entity _barracksEntity;

	private void Awake()
	{
		soldierButton.onClick.AddListener(() =>
		{
			AddUnitToSpawnQueue(UnitType.Soldier);
		});
	}

	private void OnEnable()
	{
		onBarracksSelected.RegisterListener(e => OnBarracksSelected(e.Value));
	}

	private void OnDisable()
	{
		onBarracksSelected.UnregisterListener(e => OnBarracksSelected(e.Value));
	}

	private void Start()
	{
		Hide();
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}

	private void AddUnitToSpawnQueue(UnitType unitType)
	{
		if (_barracksEntity == Entity.Null)
		{
			_entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, Barracks>().Build(_entityManager);
			if (_entityQuery.IsEmpty)
			{
				return;
			}

			using (var barracksArray = _entityQuery.ToEntityArray(Allocator.Temp))
			{
				if (barracksArray.Length > 0)
				{
					_barracksEntity = barracksArray[0];
				}
				else
				{
					return;
				}
			}
		}

		var spawnUnitTypeDynamicBuffer = _entityManager.GetBuffer<SpawnUnitTypeBuffer>(_barracksEntity);

		spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
		                               {
			                               UnitType = unitType
		                               });
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
	}

	private void Hide()
	{
		gameObject.SetActive(false);
	}

	private void Update()
	{
		UpdateProgressBar();
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