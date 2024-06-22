using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;

public abstract class OvertipAnomalyView : BaseOvertipView<OvertipEntityAnomalyVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, ISystemMapOvertip
{
	[SerializeField]
	protected OwlcatButton m_ExploreAnomalyButton;

	[Header("Anomaly Block")]
	[SerializeField]
	private TextMeshProUGUI m_AnomalyName;

	[Header("Overtip Size Control")]
	[SerializeField]
	private RectTransform m_CanvasRectTransform;

	[Header("Noise")]
	[SerializeField]
	private Image m_NoiseAroundImage;

	[SerializeField]
	private Sprite[] m_RandomNoises;

	[Header("Top Information")]
	[SerializeField]
	private TextMeshProUGUI m_TopInformation;

	[Header("Anomaly Type Identifier")]
	[SerializeField]
	private GameObject m_LootTypeIcon;

	[SerializeField]
	private GameObject m_QuestionMarkIcon;

	[SerializeField]
	private GameObject m_WrenchIcon;

	[SerializeField]
	private GameObject m_EnemyIcon;

	[SerializeField]
	private GameObject m_EnemyCircle;

	[Header("Other")]
	[SerializeField]
	private FadeAnimator m_TargetPingEntity;

	[SerializeField]
	private List<Image> m_AdditionalTargetPingImages;

	[SerializeField]
	private Image m_QuestStateIcon;

	private readonly string[] m_ObjectLatinLetters = new string[6] { "UNKNOWN", "SSI", "ENM", "GAS", "WPH", "LOT" };

	private MapObjectView m_SystemObject;

	private IDisposable m_PingDelay;

	protected override bool CheckVisibility
	{
		get
		{
			if (base.ViewModel.AnomalyView != null && base.ViewModel.AnomalyView.Data.IsInGame)
			{
				return !base.ViewModel.AnomalyView.BlueprintAnomaly.HideInUI;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_TargetPingEntity.CanvasGroup != null)
		{
			m_TargetPingEntity.CanvasGroup.alpha = 0f;
		}
		m_TargetPingEntity.DisappearAnimation();
		AddDisposable(ObservableExtensions.Subscribe(m_ExploreAnomalyButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.RequestVisit();
		}));
		base.name = base.ViewModel.SystemMapObject.View.name + "_Overtip";
		m_SystemObject = base.ViewModel.SystemMapObject.View;
		AddDisposable(EventBus.Subscribe(this));
		SetAnomalyName();
		SetOvertipSize();
		SetAnomalyTopIcon();
		AddDisposable(base.ViewModel.CoopPingEntity.Subscribe(delegate((NetPlayer player, Entity entity) value)
		{
			PingEntity(value.player, value.entity);
		}));
		AddDisposable(base.ViewModel.HasQuest.CombineLatest(base.ViewModel.QuestObjectiveName, (bool isQuest, string questObjectiveName) => new { isQuest, questObjectiveName }).Subscribe(value =>
		{
			m_QuestStateIcon.gameObject.SetActive(value.isQuest);
			if (value.isQuest && value.questObjectiveName != null)
			{
				AddDisposable(m_QuestStateIcon.SetHint(base.ViewModel.QuestObjectiveName.Value));
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_PingDelay?.Dispose();
		m_PingDelay = null;
	}

	public void SetFocus(bool value)
	{
		m_ExploreAnomalyButton.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.isActiveAndEnabled)
		{
			return base.CanvasGroup.alpha > 0f;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public void UnfocusButton()
	{
		m_ExploreAnomalyButton.OnPointerExit();
	}

	private void SetOvertipSize()
	{
		m_NoiseAroundImage.sprite = m_RandomNoises[UnityEngine.Random.Range(0, m_RandomNoises.Length)];
		m_TopInformation.text = GetTopInformationText();
		float num = UnityEngine.Random.Range(15f, 25f);
		m_TopInformation.transform.parent.transform.rotation *= Quaternion.Euler(0f, 0f, num);
		m_TopInformation.transform.rotation *= Quaternion.Euler(0f, 0f, 0f - num);
		SphereCollider component = m_SystemObject.gameObject.GetComponent<SphereCollider>();
		if (!(component == null))
		{
			Vector3 size = component.bounds.size;
			m_CanvasRectTransform.sizeDelta = new Vector2(size.x * 12f, size.y * 12f);
		}
	}

	private string GetTopInformationText()
	{
		string text = "UNKNOWN";
		if (Game.Instance.CurrentMode != GameModeType.StarSystem || !(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap))
		{
			return text;
		}
		string text2 = ((!(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap blueprintStarSystemMap)) ? text : UIUtility.ArabicToRoman(blueprintStarSystemMap.RomeSystemNumber));
		AnomalyView anomalyView = base.ViewModel.SystemMapObject.View as AnomalyView;
		string text3;
		if (anomalyView != null)
		{
			int num = Array.IndexOf(Enum.GetValues(typeof(BlueprintAnomaly.AnomalyObjectType)), anomalyView.Data.Blueprint.AnomalyType);
			text3 = ((num < 0 || num >= m_ObjectLatinLetters.Length) ? text : m_ObjectLatinLetters[num]);
		}
		else
		{
			text3 = text;
		}
		string text4 = UnityEngine.Random.Range(100, 1000).ToString();
		return "Notitium collectionis:\n// RES //\n" + text2 + "-" + text3 + "-" + text4;
	}

	private void SetAnomalyName()
	{
		m_AnomalyName.text = (string.IsNullOrWhiteSpace(base.ViewModel.AnomalyName.Value) ? "Empty Name" : base.ViewModel.AnomalyName.Value);
	}

	private void SetAnomalyTopIcon()
	{
		AnomalyView anomalyView = base.ViewModel.SystemMapObject.View as AnomalyView;
		if (anomalyView == null)
		{
			return;
		}
		BlueprintAnomaly.AnomalyObjectType anomalyType = anomalyView.Data.Blueprint.AnomalyType;
		Dictionary<BlueprintAnomaly.AnomalyObjectType, GameObject> dictionary = new Dictionary<BlueprintAnomaly.AnomalyObjectType, GameObject>
		{
			{
				BlueprintAnomaly.AnomalyObjectType.Loot,
				m_LootTypeIcon
			},
			{
				BlueprintAnomaly.AnomalyObjectType.Enemy,
				m_EnemyIcon
			},
			{
				BlueprintAnomaly.AnomalyObjectType.ShipSignature,
				m_QuestionMarkIcon
			},
			{
				BlueprintAnomaly.AnomalyObjectType.Gas,
				m_QuestionMarkIcon
			},
			{
				BlueprintAnomaly.AnomalyObjectType.WarpHton,
				m_QuestionMarkIcon
			},
			{
				BlueprintAnomaly.AnomalyObjectType.Default,
				m_QuestionMarkIcon
			}
		};
		foreach (GameObject value2 in dictionary.Values)
		{
			value2.SetActive(value: false);
		}
		if (dictionary.TryGetValue(anomalyType, out var value))
		{
			value.SetActive(value: true);
		}
		m_EnemyCircle.SetActive(anomalyType == BlueprintAnomaly.AnomalyObjectType.Enemy);
	}

	private void PingEntity(NetPlayer player, Entity entity)
	{
		m_PingDelay?.Dispose();
		if (entity != base.ViewModel.SystemMapObject)
		{
			m_TargetPingEntity.DisappearAnimation();
			return;
		}
		int index = player.Index - 1;
		Image component = m_TargetPingEntity.GetComponent<Image>();
		Color currentColor = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
		if (component != null)
		{
			component.color = currentColor;
		}
		if (m_AdditionalTargetPingImages != null && m_AdditionalTargetPingImages.Any())
		{
			m_AdditionalTargetPingImages.ForEach(delegate(Image i)
			{
				i.color = currentColor;
			});
		}
		m_TargetPingEntity.AppearAnimation();
		m_PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingEntity.DisappearAnimation();
		}, 7.5f);
	}
}
