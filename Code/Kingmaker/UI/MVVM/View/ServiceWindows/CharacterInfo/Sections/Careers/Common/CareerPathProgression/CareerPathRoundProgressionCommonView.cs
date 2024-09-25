using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;

public class CareerPathRoundProgressionCommonView : ViewBase<CareerPathVM>, IUIHighlighter, ISubscriber
{
	[Header("Progress Bar")]
	[SerializeField]
	private RectTransform m_ProgressBarContainer;

	[SerializeField]
	private Image m_AppliedProgressBar;

	[SerializeField]
	private Image m_CurrentProgressBar;

	[SerializeField]
	private Image m_CurrentPassedProgressBar;

	[Header("Rank Entries")]
	[SerializeField]
	private RectTransform m_RankEntriesContainer;

	[FormerlySerializedAs("m_CareerPathRankEntryPCView")]
	[SerializeField]
	private RankEntryItemCommonView m_CareerPathRankEntryCommonView;

	[FormerlySerializedAs("m_CareerPathListItemPCView")]
	[Header("Common")]
	[SerializeField]
	private CareerPathListItemCommonView m_CareerPathListItemCommonView;

	[SerializeField]
	private CareerPathRoundProgressionConfig[] Configs;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	public bool IsShip;

	[ConditionalShow("IsShip")]
	[SerializeField]
	private Image m_ShipIcon;

	private readonly List<RankEntryItemCommonView> m_RankEntries = new List<RankEntryItemCommonView>();

	private readonly Dictionary<int, float> m_RankToAngle = new Dictionary<int, float>();

	private const float StartAngle = MathF.PI / 2f;

	private FloatConsoleNavigationBehaviour m_NavigationBehaviour;

	private RectTransform m_TooltipPlace;

	public RectTransform RectTransform => m_ProgressBarContainer;

	public void Initialize()
	{
		ClearBars();
		Hide();
	}

	public void SetViewParameters(RectTransform tooltipPlace)
	{
		m_TooltipPlace = tooltipPlace;
	}

	protected override void BindViewImplementation()
	{
		m_CareerPathListItemCommonView.SetViewParameters(m_TooltipPlace);
		m_CareerPathListItemCommonView.Bind(base.ViewModel);
		DrawEntries();
		UpdateProgressBar();
		UpdateCurrentProgressBar();
		Show();
		AddDisposable(base.ViewModel.OnCommit.Subscribe(delegate
		{
			UpdateProgressBar();
		}));
		AddDisposable(base.ViewModel.PointerItem.Subscribe(delegate
		{
			UpdateCurrentProgressBar();
		}));
		AddDisposable(base.ViewModel.OnUpdateData.Subscribe(delegate
		{
			UpdateProgressBar();
		}));
		AddDisposable(EventBus.Subscribe(this));
		if (IsShip && base.ViewModel.PlayerShipSprite != null && m_ShipIcon != null)
		{
			m_ShipIcon.sprite = base.ViewModel.PlayerShipSprite;
		}
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
		Hide();
	}

	private void Clear()
	{
		m_RankEntries.ForEach(WidgetFactory.DisposeWidget);
		m_RankEntries.Clear();
		ClearBars();
	}

	private void ClearBars()
	{
		m_CurrentPassedProgressBar.fillAmount = 0f;
		m_AppliedProgressBar.fillAmount = 0f;
		m_CurrentProgressBar.fillAmount = 0f;
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void DrawEntries()
	{
		CareerPathRoundProgressionConfig config = GetConfig();
		int count = base.ViewModel.RankEntries.Count;
		float num = MathF.PI * 2f / (float)count;
		float num2 = MathF.PI / 2f;
		for (int i = 0; i < count; i++)
		{
			CareerPathRankEntryVM careerPathRankEntryVM = base.ViewModel.RankEntries[i];
			RankEntryItemCommonView widget = WidgetFactory.GetWidget(m_CareerPathRankEntryCommonView);
			widget.transform.SetParent(m_RankEntriesContainer, worldPositionStays: false);
			widget.SetViewParameters(m_TooltipPlace);
			widget.Bind(careerPathRankEntryVM);
			m_RankEntries.Add(widget);
			widget.transform.localPosition = new Vector3(Mathf.Cos(num2), Mathf.Sin(num2)) * config.ItemsRadius;
			float num3 = (num2 - MathF.PI / 2f) * 57.29578f;
			widget.SetRotation(num3);
			m_RankToAngle[careerPathRankEntryVM.Rank] = num3;
			num2 -= num;
		}
		m_ProgressBarContainer.sizeDelta = Vector2.one * config.ProgressBarSize;
		UpdateNavigation();
	}

	private void UpdateCurrentProgressBar()
	{
		DOTween.Kill(m_CurrentPassedProgressBar);
		if (!(base.ViewModel.UnitProgressionVM?.CurrentCareer?.Value.IsInLevelupProcess).GetValueOrDefault())
		{
			return;
		}
		(int, int) currentLevelupRange = base.ViewModel.GetCurrentLevelupRange();
		if (base.ViewModel.PointerItem.Value == null)
		{
			int num = (base.ViewModel.IsUnlocked ? (currentLevelupRange.Item2 - 1) : 0);
			m_CurrentPassedProgressBar.fillAmount = (float)num / (float)base.ViewModel.MaxRank;
			return;
		}
		float num2 = (float)(base.ViewModel.PointerItem.Value.EntryRank - 1) / (float)base.ViewModel.MaxRank;
		if (Math.Abs(m_CurrentPassedProgressBar.fillAmount - num2) > 0.001f)
		{
			m_CurrentPassedProgressBar.DOFillAmount(num2, 0.25f).SetUpdate(isIndependentUpdate: true);
		}
	}

	private void UpdateProgressBar()
	{
		if (base.ViewModel.IsFinished)
		{
			Image appliedProgressBar = m_AppliedProgressBar;
			float fillAmount = (m_CurrentProgressBar.fillAmount = 1f);
			appliedProgressBar.fillAmount = fillAmount;
			return;
		}
		if (!base.ViewModel.IsUnlocked)
		{
			Image appliedProgressBar2 = m_AppliedProgressBar;
			float fillAmount = (m_CurrentProgressBar.fillAmount = 0f);
			appliedProgressBar2.fillAmount = fillAmount;
			return;
		}
		(int, int) currentLevelupRange = base.ViewModel.GetCurrentLevelupRange();
		int num3 = ((currentLevelupRange.Item1 == -1) ? (base.ViewModel.CurrentRank.Value - 1) : (currentLevelupRange.Item1 - 2));
		int num4 = ((currentLevelupRange.Item2 == base.ViewModel.MaxRank) ? base.ViewModel.MaxRank : (currentLevelupRange.Item2 - 1));
		m_AppliedProgressBar.fillAmount = (float)num3 / (float)base.ViewModel.MaxRank;
		if ((base.ViewModel.UnitProgressionVM?.CurrentCareer?.Value.IsInLevelupProcess).GetValueOrDefault())
		{
			m_CurrentProgressBar.fillAmount = (float)num4 / (float)base.ViewModel.MaxRank;
		}
	}

	private CareerPathRoundProgressionConfig GetConfig()
	{
		return Configs.First((CareerPathRoundProgressionConfig i) => i.Tier == base.ViewModel.CareerPath.Tier);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			return;
		}
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddEntity(m_CareerPathListItemCommonView);
		if (m_RankEntries.Any())
		{
			m_NavigationBehaviour.AddEntities(m_RankEntries.SelectMany((RankEntryItemCommonView i) => i.GetConsoleEntities()).ToList());
		}
		IRankEntrySelectItem currentRankEntryItem = base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value;
		IFloatConsoleNavigationEntity floatConsoleNavigationEntity = m_RankEntries.Select((RankEntryItemCommonView i) => i.TryGetItemByViewModel(currentRankEntryItem)).FirstOrDefault((IFloatConsoleNavigationEntity i) => i != null);
		m_NavigationBehaviour.SetCurrentEntity(floatConsoleNavigationEntity ?? m_CareerPathListItemCommonView);
	}

	public FloatConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation();
		}
		return m_NavigationBehaviour;
	}

	public void StartHighlight(string key)
	{
		m_RankEntries.ForEach(delegate(RankEntryItemCommonView re)
		{
			re.SetHighlightStateToItemsWithKey(key, state: true);
		});
	}

	public void StopHighlight(string key)
	{
		m_RankEntries.ForEach(delegate(RankEntryItemCommonView re)
		{
			re.SetHighlightStateToItemsWithKey(key, state: false);
		});
	}

	public void Highlight(string key)
	{
	}

	public void HighlightOnce(string key)
	{
	}
}
