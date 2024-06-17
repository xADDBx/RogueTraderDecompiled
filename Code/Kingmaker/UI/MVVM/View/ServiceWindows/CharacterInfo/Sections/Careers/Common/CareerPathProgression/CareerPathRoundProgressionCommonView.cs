using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;

public class CareerPathRoundProgressionCommonView : ViewBase<CareerPathVM>
{
	[Header("Progress Bar")]
	[SerializeField]
	private RectTransform m_ProgressBarContainer;

	[SerializeField]
	private Image m_AppliedProgressBar;

	[SerializeField]
	private Image m_CurrentProgressBar;

	[SerializeField]
	private Image m_ProgressBackground;

	[Header("Rank Entries")]
	[SerializeField]
	private RectTransform m_RankEntriesContainer;

	[FormerlySerializedAs("m_CareerPathRankEntryPCView")]
	[SerializeField]
	private RankEntryItemCommonView m_CareerPathRankEntryCommonView;

	[Header("Pointer")]
	[SerializeField]
	private RectTransform m_PointerTransform;

	[SerializeField]
	private FadeAnimator m_PointerAnimator;

	[SerializeField]
	private float m_PointerRotationDuration = 0.5f;

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

	private Action<RectTransform> m_EnsureVisibleAction;

	public RectTransform MainRankEntry => m_CareerPathListItemCommonView.GetComponent<RectTransform>();

	public RectTransform GetCurrentSelectedRect()
	{
		if (m_CareerPathListItemCommonView.IsSelectedForUI())
		{
			return m_CareerPathListItemCommonView.transform as RectTransform;
		}
		RankEntryItemCommonView rankEntryItemCommonView = m_RankEntries.FirstOrDefault((RankEntryItemCommonView entry) => entry.IsInSelectionProcess);
		if (!(rankEntryItemCommonView != null))
		{
			return null;
		}
		return rankEntryItemCommonView.transform as RectTransform;
	}

	public void Initialize()
	{
		m_PointerAnimator.Initialize();
		Hide();
	}

	public void SetViewParameters(RectTransform tooltipPlace, Action<RectTransform> ensureVisibleAction)
	{
		m_TooltipPlace = tooltipPlace;
		m_EnsureVisibleAction = ensureVisibleAction;
	}

	protected override void BindViewImplementation()
	{
		m_CareerPathListItemCommonView.SetViewParameters(m_TooltipPlace, m_EnsureVisibleAction);
		m_CareerPathListItemCommonView.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.OnUpdateData.Subscribe(delegate
		{
			UpdateProgressBar();
		}));
		DrawEntries();
		UpdateProgressBar();
		Show();
		AddDisposable(base.ViewModel.PointerItem.Subscribe(SetPointerAt));
		AddDisposable(base.ViewModel.OnCommit.Subscribe(delegate
		{
			SetPointerAt(null);
		}));
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
			widget.SetViewParameters(m_TooltipPlace, m_EnsureVisibleAction);
			widget.Bind(careerPathRankEntryVM);
			m_RankEntries.Add(widget);
			widget.transform.localPosition = new Vector3(Mathf.Cos(num2) * (float)config.ItemsRadius, Mathf.Sin(num2) * (float)config.ItemsRadius);
			float num3 = (num2 - MathF.PI / 2f) * 57.29578f;
			widget.SetRotation(num3);
			m_RankToAngle[careerPathRankEntryVM.Rank] = num3;
			num2 -= num;
		}
		m_ProgressBarContainer.sizeDelta = new Vector2(config.ProgressBarSize, config.ProgressBarSize);
		if ((bool)config.Icon)
		{
			m_ProgressBackground.rectTransform.sizeDelta = config.Icon.rect.size;
			m_ProgressBackground.sprite = config.Icon;
		}
		UpdateNavigation();
	}

	private void UpdateProgressBar()
	{
		if (base.ViewModel.IsFinished)
		{
			Image appliedProgressBar = m_AppliedProgressBar;
			float fillAmount = (m_CurrentProgressBar.fillAmount = 1f);
			appliedProgressBar.fillAmount = fillAmount;
		}
		else if (!base.ViewModel.IsAvailableToUpgrade && !base.ViewModel.IsInProgress)
		{
			Image appliedProgressBar2 = m_AppliedProgressBar;
			float fillAmount = (m_CurrentProgressBar.fillAmount = 0f);
			appliedProgressBar2.fillAmount = fillAmount;
		}
		else
		{
			(int, int) currentLevelupRange = base.ViewModel.GetCurrentLevelupRange();
			int num3 = ((currentLevelupRange.Item1 == -1) ? (base.ViewModel.CurrentRank.Value - 1) : (currentLevelupRange.Item1 - 2));
			int num4 = ((currentLevelupRange.Item2 == base.ViewModel.MaxRank) ? base.ViewModel.MaxRank : (currentLevelupRange.Item2 - 1));
			m_AppliedProgressBar.fillAmount = (float)num3 / (float)base.ViewModel.MaxRank;
			m_CurrentProgressBar.fillAmount = (float)num4 / (float)base.ViewModel.MaxRank;
		}
	}

	private void SetPointerAt(IRankEntrySelectItem item)
	{
		int num = ((item is RankEntryFeatureItemVM { Rank: var rank }) ? (rank ?? (-1)) : ((!(item is RankEntrySelectionVM rankEntrySelectionVM)) ? (-1) : rankEntrySelectionVM.Rank));
		int key = num;
		if (m_RankToAngle.TryGetValue(key, out var value))
		{
			m_PointerTransform.Or(null)?.DORotate(new Vector3(0f, 0f, value), m_PointerRotationDuration).SetUpdate(isIndependentUpdate: true);
			m_PointerAnimator.AppearAnimation();
		}
		else
		{
			m_PointerAnimator.DisappearAnimation();
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
}
