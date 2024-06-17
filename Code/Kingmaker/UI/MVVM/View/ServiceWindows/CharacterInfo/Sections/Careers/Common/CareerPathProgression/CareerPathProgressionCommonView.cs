using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;

public class CareerPathProgressionCommonView : ViewBase<CareerPathVM>
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected CareerPathSelectionTabsCommonView m_CareerPathSelectionPartCommonView;

	[SerializeField]
	private CharInfoAvailableRanksCommonView m_AvailableRanksCommonView;

	[SerializeField]
	protected CareerPathRoundProgressionCommonView m_CareerPathRoundProgressionCommonView;

	[SerializeField]
	protected InfoSectionView m_InfoSection;

	private Action<bool> m_ReturnAction;

	protected readonly ReactiveProperty<bool> m_IsShown = new ReactiveProperty<bool>();

	[Header("SizeChanger")]
	[SerializeField]
	protected bool m_CanExpand = true;

	[SerializeField]
	protected bool m_AlwaysShowInfo;

	[SerializeField]
	[ConditionalShow("m_CanExpand")]
	private SizeAnimator m_ProgressionSizeAnimator;

	[SerializeField]
	[ConditionalShow("m_CanExpand")]
	protected SizeAnimator m_DescriptionSizeAnimator;

	[SerializeField]
	[ConditionalShow("m_CanExpand")]
	private OwlcatSelectable m_ProgressionArea;

	[Header("Scroll")]
	[SerializeField]
	private ScrollRectExtended m_CareerScrollRectExtended;

	[SerializeField]
	private float m_EnsureVisibleSenseZone = 350f;

	[Header("Scroll")]
	[SerializeField]
	private RectTransform m_TooltipPlace;

	[Header("AlreadyInLevelUp")]
	[SerializeField]
	private TextMeshProUGUI m_WarningText;

	[SerializeField]
	private GameObject m_WarningContainer;

	[SerializeField]
	protected OwlcatMultiButton m_AttentionSign;

	private bool m_IsInAnimation;

	private bool m_SizeSettedFromUser;

	protected AccessibilityTextHelper TextHelper;

	public virtual void Initialize(Action<bool> returnAction)
	{
		m_ReturnAction = returnAction;
		m_FadeAnimator.Initialize();
		m_CareerPathSelectionPartCommonView.Initialize();
		m_DescriptionSizeAnimator.Or(null)?.Initialize();
		m_ProgressionSizeAnimator.Or(null)?.Initialize();
		m_CareerPathRoundProgressionCommonView.SetViewParameters(m_TooltipPlace, delegate(RectTransform rectTransform)
		{
			EnsureVisible(rectTransform);
		});
		TextHelper = new AccessibilityTextHelper(m_WarningText);
	}

	protected override void BindViewImplementation()
	{
		m_AvailableRanksCommonView.Or(null)?.Bind(base.ViewModel.AvailableRanksVM);
		m_CareerPathSelectionPartCommonView.Bind(base.ViewModel);
		m_CareerPathRoundProgressionCommonView.Bind(base.ViewModel);
		m_InfoSection.Bind(base.ViewModel.SelectedItemInfoSectionVM);
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_ProgressionArea.Or(null)?.OnPointerClickAsObservable().Subscribe(delegate
		{
			SwitchDescriptionShowed();
		}));
		m_SizeSettedFromUser = false;
		AddDisposable(base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Subscribe(UpdateResizable));
		EnsureVisible(m_CareerPathRoundProgressionCommonView.GetCurrentSelectedRect(), smooth: false);
		m_ProgressionSizeAnimator.Or(null)?.SetOnUpdateAction(OnResizeUpdate);
		AddDisposable(base.ViewModel.IsDescriptionShowed.Subscribe(AnimateSwitchDescription));
		m_WarningText.text = UIStrings.Instance.CharacterSheet.AlreadyInLevelUp;
		bool active = base.ViewModel.IsAvailableToUpgrade && base.ViewModel.AlreadyInLevelupOther;
		m_WarningContainer.Or(null)?.SetActive(active);
		m_AttentionSign.Or(null)?.gameObject.SetActive(active);
		if (m_AlwaysShowInfo)
		{
			m_InfoSection.SetActive(state: true);
		}
		TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_SizeSettedFromUser = false;
		m_ProgressionSizeAnimator.Or(null)?.SetDisappearSize();
		m_DescriptionSizeAnimator.Or(null)?.SetDisappearSize();
		EnsureVisible(m_CareerPathRoundProgressionCommonView.MainRankEntry, smooth: false);
		m_CareerPathSelectionPartCommonView.Unbind();
		m_CareerPathRoundProgressionCommonView.Unbind();
		m_WarningContainer.Or(null)?.SetActive(value: false);
		m_AttentionSign.Or(null)?.gameObject.SetActive(value: false);
		m_InfoSection.Unbind();
		TextHelper.Dispose();
	}

	public void SetVisibility(bool visible)
	{
		m_FadeAnimator.PlayAnimation(visible);
		m_IsShown.Value = visible;
	}

	protected void HandleReturn()
	{
		m_ReturnAction?.Invoke(obj: false);
	}

	protected void HandleUpdateDescriptionView()
	{
		SwitchDescriptionShowed();
		m_SizeSettedFromUser = true;
	}

	public void UpdateResizable(IRankEntrySelectItem item)
	{
		if (!m_SizeSettedFromUser)
		{
			SwitchDescriptionShowed(item?.CanSelect() ?? false);
		}
	}

	protected void SwitchDescriptionShowed(bool? state = null)
	{
		if (m_CanExpand)
		{
			base.ViewModel.SwitchDescriptionShowed(state);
		}
	}

	public void AnimateSwitchDescription(bool state)
	{
		m_IsInAnimation = true;
		m_InfoSection.SetActive(state || m_AlwaysShowInfo);
		if (state)
		{
			m_DescriptionSizeAnimator.Or(null)?.AppearAnimation();
			m_ProgressionSizeAnimator.Or(null)?.AppearAnimation(delegate
			{
				m_IsInAnimation = false;
			});
		}
		else
		{
			m_DescriptionSizeAnimator.Or(null)?.DisappearAnimation(delegate
			{
				m_IsInAnimation = false;
			});
			m_ProgressionSizeAnimator.Or(null)?.DisappearAnimation();
		}
	}

	private void OnResizeUpdate()
	{
		if ((bool)m_ProgressionSizeAnimator)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_ProgressionSizeAnimator.transform as RectTransform);
		}
		EnsureVisible(m_CareerPathRoundProgressionCommonView.GetCurrentSelectedRect(), smooth: false);
	}

	public void EnsureVisible(RectTransform rectTransform, bool smooth = true)
	{
		if (!smooth || !m_IsInAnimation)
		{
			m_CareerScrollRectExtended.EnsureVisibleHorizontal(rectTransform, m_EnsureVisibleSenseZone, smooth);
		}
	}
}
