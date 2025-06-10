using System.Collections;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.MVVM.VM.CombatLog;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CombatLog.PC;

public class CombatLogPCView : CombatLogBaseView
{
	[SerializeField]
	private OwlcatMultiButton m_SwitchPinButton;

	[SerializeField]
	private OwlcatButton m_ForceScrollToBottomButton;

	[Header("Resizable")]
	[SerializeField]
	private ResizePanel m_ResizePanel;

	[SerializeField]
	private OwlcatButton m_ResizePanelButton;

	[Header("Filters")]
	[SerializeField]
	private FadeAnimator m_FiltersAnimator;

	[SerializeField]
	private FadeAnimator[] m_FiltersAnimators;

	[SerializeField]
	private Image[] m_FilterArrows;

	private IReadOnlyReactiveProperty<bool> m_IsFiltersVisible;

	private readonly BoolReactiveProperty m_IsMoseHovered = new BoolReactiveProperty();

	private bool m_BottomEdgeVisible;

	private bool BottomEdgeNeeded
	{
		get
		{
			if (Application.isPlaying)
			{
				if (base.ViewModel.Items.Count > 0)
				{
					ReactiveCollection<CombatLogBaseVM> items = base.ViewModel.Items;
					return !items[items.Count - 1].HasView;
				}
				return false;
			}
			return true;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_FiltersAnimator.Initialize();
		m_FiltersAnimator.gameObject.SetActive(value: false);
		FadeAnimator[] filtersAnimators = m_FiltersAnimators;
		foreach (FadeAnimator obj in filtersAnimators)
		{
			obj.Initialize();
			obj.gameObject.SetActive(value: false);
		}
		m_ResizePanel.Initialize(this);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetSizeDelta(Game.Instance.Player.UISettings.LogSize);
		AddDisposable(UniRxExtensionMethods.Subscribe(m_ForceScrollToBottomButton.OnLeftClickAsObservable(), delegate
		{
			m_VirtualList.ScrollController.ForceScrollToBottom();
		}));
		foreach (CombatLogToggleWithCustomHint toggle in m_Toggles)
		{
			AddDisposable(toggle.SetHint(base.ViewModel.GetChannelName(m_Toggles.IndexOf(toggle))));
		}
		AddDisposable(m_ToggleGroup.ActiveToggle.Subscribe(delegate(OwlcatToggle toggle)
		{
			HandleActiveToggleChanged(toggle as CombatLogToggleWithCustomHint);
		}));
		m_IsFiltersVisible = IsPinned.Or(m_IsMoseHovered).ToReadOnlyReactiveProperty();
		AddDisposable(m_IsFiltersVisible.Subscribe(SwitchFiltersVisibility));
		UISounds.Instance.SetClickAndHoverSound(m_SwitchPinButton, UISounds.ButtonSoundsEnum.PlastickSound);
		if (m_ResizePanelButton != null)
		{
			UISounds.Instance.SetClickAndHoverSound(m_ResizePanelButton, UISounds.ButtonSoundsEnum.PlastickSound);
		}
		AddDisposable(UniRxExtensionMethods.Subscribe(m_SwitchPinButton.OnLeftClickAsObservable(), delegate
		{
			IsPinned.Value = !IsPinned.Value;
		}));
		AddDisposable(m_SwitchPinButton.SetHint(UIStrings.Instance.CombatTexts.CombatLogShowHide, "ShowHideCombatLog"));
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(OnLateUpdate));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.ShowHideCombatLog.name, delegate
		{
			IsPinned.Value = !IsPinned.Value;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		StopAllCoroutines();
	}

	protected virtual void OnLateUpdate()
	{
		m_BottomEdgeVisible = VisibleEdge(m_BottomEdgeVisible, BottomEdgeNeeded, m_ForceScrollToBottomButton);
	}

	private bool VisibleEdge(bool visible, bool condition, OwlcatButton button)
	{
		if (button == null)
		{
			return visible;
		}
		if (visible == condition)
		{
			return visible;
		}
		button.gameObject.SetActive(condition);
		visible = condition;
		return visible;
	}

	public void ChangeOwnPosition(float pos)
	{
		((RectTransform)base.transform).DOAnchorPosY(pos, 0.2f);
	}

	public void SwitchFiltersVisibility(bool state)
	{
		StopAllCoroutines();
		if (state)
		{
			m_FiltersAnimator.AppearAnimation();
			StartCoroutine(PlayFiltersAppearAnimationCoroutine());
		}
		else
		{
			m_FiltersAnimator.DisappearAnimation();
			StartCoroutine(PlayFiltersDisappearAnimationCoroutine());
		}
	}

	private IEnumerator PlayFiltersAppearAnimationCoroutine()
	{
		UISounds.Instance.Sounds.CombatLog.CombatLogFiltersOpen.Play();
		for (int i = 0; i < m_FiltersAnimators.Length; i++)
		{
			yield return new WaitForSeconds(0.1f);
			m_FiltersAnimators[i].AppearAnimation();
		}
	}

	private IEnumerator PlayFiltersDisappearAnimationCoroutine()
	{
		UISounds.Instance.Sounds.CombatLog.CombatLogFiltersClose.Play();
		for (int i = m_FiltersAnimators.Length - 1; i >= 0; i--)
		{
			yield return new WaitForSeconds(0.1f);
			m_FiltersAnimators[i].DisappearAnimation();
		}
	}

	public override void SwitchPinnedState(bool pinned)
	{
		base.SwitchPinnedState(pinned);
		if (pinned)
		{
			m_SwitchPinButton.SetActiveLayer("Pinned");
			SetContainerState(state: true);
		}
		else
		{
			m_SwitchPinButton.SetActiveLayer("NotPinned");
		}
	}

	private void HandleActiveToggleChanged(CombatLogToggleWithCustomHint active)
	{
		if (active == null)
		{
			return;
		}
		int num = m_Toggles.IndexOf(active);
		base.ViewModel.SetCurrentChannelById(num);
		for (int i = 0; i < m_FilterArrows.Length; i++)
		{
			Image image = m_FilterArrows[i];
			if (i != num)
			{
				image.enabled = false;
				continue;
			}
			image.enabled = true;
			StartedTweeners.Add(image.rectTransform.DOShakeScale(0.1f, 1f, 10, 45f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		}
	}
}
