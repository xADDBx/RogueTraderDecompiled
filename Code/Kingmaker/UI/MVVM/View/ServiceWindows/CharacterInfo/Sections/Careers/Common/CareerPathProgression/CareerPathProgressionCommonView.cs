using System;
using System.Threading.Tasks;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;

public class CareerPathProgressionCommonView : ViewBase<CareerPathVM>
{
	[Space]
	[SerializeField]
	private bool m_ExpandImmediate;

	[Space]
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
	protected bool m_CanMove = true;

	[SerializeField]
	[ConditionalShow("m_CanMove")]
	protected ScrollRectExtended m_CareerScrollRectExtended;

	[ConditionalShow("m_CanMove")]
	[SerializeField]
	private ContentSizeFitter m_ContentSizeFitter;

	[SerializeField]
	[ConditionalShow("m_CanMove")]
	protected float m_MoveAnimationTime = 0.5f;

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

	private Tween m_MoveTween;

	protected AccessibilityTextHelper TextHelper;

	public virtual void Initialize(Action<bool> returnAction)
	{
		m_ReturnAction = returnAction;
		m_FadeAnimator.Initialize();
		m_CareerPathSelectionPartCommonView.Initialize();
		m_CareerPathRoundProgressionCommonView.Initialize();
		m_CareerPathRoundProgressionCommonView.SetViewParameters(m_TooltipPlace);
		TextHelper = new AccessibilityTextHelper(m_WarningText);
	}

	protected override void BindViewImplementation()
	{
		m_AvailableRanksCommonView.Or(null)?.Bind(base.ViewModel.AvailableRanksVM);
		m_CareerPathSelectionPartCommonView.Bind(base.ViewModel);
		m_CareerPathRoundProgressionCommonView.Bind(base.ViewModel);
		m_InfoSection.Bind(base.ViewModel.SelectedItemInfoSectionVM);
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.IsDescriptionShowed.Skip(1).Subscribe(AnimateSwitchDescription));
		DelayedInvoker.InvokeInFrames(delegate
		{
			AnimateSwitchDescription(base.ViewModel?.IsDescriptionShowed.Value ?? false);
		}, 1);
		m_WarningText.text = UIStrings.Instance.CharacterSheet.AlreadyInLevelUp;
		bool active = base.ViewModel.IsAvailableToUpgrade && base.ViewModel.AlreadyInLevelupOther;
		m_WarningContainer.Or(null)?.SetActive(active);
		m_AttentionSign.Or(null)?.gameObject.SetActive(active);
		TextHelper.UpdateTextSize();
		if (m_ExpandImmediate)
		{
			base.ViewModel.SwitchDescriptionShowed(true);
		}
		CorrectContentSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_CareerScrollRectExtended.horizontalNormalizedPosition = 0f;
		m_CareerPathSelectionPartCommonView.Unbind();
		m_CareerPathRoundProgressionCommonView.Unbind();
		m_WarningContainer.Or(null)?.SetActive(value: false);
		m_AttentionSign.Or(null)?.gameObject.SetActive(value: false);
		m_InfoSection.Unbind();
		DOTween.Kill(m_MoveTween);
		TextHelper.Dispose();
		DOTween.Kill(m_MoveTween);
		if ((bool)m_ContentSizeFitter)
		{
			m_ContentSizeFitter.enabled = false;
		}
		SetDefaultSize();
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

	protected void SwitchDescriptionShowed(bool? state = null)
	{
		if (m_CanMove)
		{
			base.ViewModel.SwitchDescriptionShowed(state);
		}
	}

	public void AnimateSwitchDescription(bool state)
	{
		float num = (state ? 1f : 0f);
		if (Math.Abs(num - m_CareerScrollRectExtended.horizontalNormalizedPosition) > 0.001f)
		{
			DOTween.Kill(m_MoveTween);
			m_MoveTween = DOTween.To(() => m_CareerScrollRectExtended.horizontalNormalizedPosition, delegate(float x)
			{
				m_CareerScrollRectExtended.horizontalNormalizedPosition = x;
			}, num, m_MoveAnimationTime).SetUpdate(isIndependentUpdate: true);
		}
	}

	private async void CorrectContentSize()
	{
		if (!m_ContentSizeFitter)
		{
			return;
		}
		await Task.Yield();
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_CareerScrollRectExtended.content);
		float num = 0f;
		for (int i = 0; i < m_CareerScrollRectExtended.content.childCount; i++)
		{
			Transform child = m_CareerScrollRectExtended.content.GetChild(i);
			LayoutElement component = child.GetComponent<LayoutElement>();
			if (!(component != null) || !component.ignoreLayout)
			{
				num += child.GetComponent<RectTransform>().rect.width;
			}
		}
		float num2 = 0.1f;
		float width = m_CareerScrollRectExtended.GetComponent<RectTransform>().rect.width;
		bool flag = num < width + num2;
		m_ContentSizeFitter.enabled = !flag;
		if (flag)
		{
			SetDefaultSize();
		}
	}

	private void SetDefaultSize()
	{
		m_CareerScrollRectExtended.content.anchoredPosition = Vector2.zero;
		m_CareerScrollRectExtended.content.sizeDelta = Vector2.zero;
	}
}
