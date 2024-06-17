using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil.Console;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Inspect;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class SurfaceActionBarConsoleView : ViewBase<SurfaceActionBarVM>, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Parts")]
	[SerializeField]
	private SurfaceActionBarPartQuickAccessConsoleView m_QuickAccessConsoleView;

	[SerializeField]
	private SurfaceActionBarPartAbilitiesConsoleView m_AbilitiesConsoleView;

	[SerializeField]
	private SurfaceMomentumConsoleView m_MomentumConsoleView;

	[SerializeField]
	private VeilThicknessConsoleView m_VeilThicknessConsoleView;

	[Header("Alerts")]
	[SerializeField]
	private Image m_ClearMPAlertGroup;

	[SerializeField]
	private Image m_AttackAbilityGroupCooldownAlertGroup;

	private readonly ReactiveProperty<bool> m_IsInspectVisible = new ReactiveProperty<bool>();

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_InspectHint;

	[SerializeField]
	private ConsoleHint m_HideActionBarHint;

	[SerializeField]
	private ConsoleHint m_ShowActionBarHint;

	[Header("ContainersForMarkers")]
	[SerializeField]
	private GameObject[] m_ContainersForMarkers;

	private IReadOnlyReactiveProperty<bool> m_IsVisible;

	private BoolReactiveProperty m_VisibleTrigger;

	public void Initialize()
	{
		m_QuickAccessConsoleView.Initialize();
		m_AbilitiesConsoleView.Initialize();
		m_MomentumConsoleView.Initialize();
		m_VeilThicknessConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		m_VisibleTrigger = new BoolReactiveProperty();
		m_IsVisible = base.ViewModel.IsVisible.And(m_VisibleTrigger).ToReactiveProperty();
		m_QuickAccessConsoleView.Bind(base.ViewModel);
		m_AbilitiesConsoleView.Bind(base.ViewModel.Abilities);
		m_MomentumConsoleView.Bind(base.ViewModel.SurfaceMomentumVM);
		m_VeilThicknessConsoleView.Bind(base.ViewModel.VeilThickness);
		HandleTurnBasedModeSwitched(TurnController.IsInTurnBasedCombat());
		AddDisposable(m_IsVisible.Subscribe(OnVisibleChanged));
		AddDisposable(base.ViewModel.CurrentCombatUnit.Subscribe(delegate
		{
			if (m_IsVisible.Value)
			{
				UISounds.Instance.Sounds.ActionBar.ActionBarSwitch.Play();
			}
		}));
		AddDisposable(base.ViewModel.EndTurnText.Subscribe(delegate(string value)
		{
			m_ClearMPAlertGroup.gameObject.SetActive(!string.IsNullOrEmpty(value));
		}));
		AddDisposable(base.ViewModel.IsAttackAbilityGroupCooldownAlertActive.Subscribe(m_AttackAbilityGroupCooldownAlertGroup.gameObject.SetActive));
		AddDisposable(base.ViewModel.HighlightedUnit.Subscribe(OnHighlightedUnit));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void AddInput(InputLayer inputLayer, bool inCombat)
	{
		m_QuickAccessConsoleView.AddInput(inputLayer, m_IsVisible, inCombat);
		m_AbilitiesConsoleView.AddInput(inputLayer, m_IsVisible, inCombat);
		AddDisposable(m_InspectHint.Bind(inputLayer.AddButton(delegate
		{
			OnInspectUnit();
		}, 19, m_IsInspectVisible, InputActionEventType.ButtonJustLongPressed)));
		m_InspectHint.SetLabel(UIStrings.Instance.MainMenu.Inspect);
		if (!inCombat)
		{
			AddDisposable(m_HideActionBarHint.Bind(inputLayer.AddButton(delegate
			{
				TriggerVisibility(trigger: false);
			}, 9, m_IsVisible)));
			m_HideActionBarHint.SetLabel(UIStrings.Instance.HUDTexts.HideActionBar);
			IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = m_IsVisible.Not().ToReactiveProperty();
			AddDisposable(m_ShowActionBarHint.Bind(inputLayer.AddButton(delegate
			{
				TriggerVisibility(trigger: true);
			}, 11, readOnlyReactiveProperty)));
			m_ShowActionBarHint.SetLabel(UIStrings.Instance.HUDTexts.ShowActionBar);
		}
	}

	private void TriggerVisibility(bool trigger)
	{
		if (!(Game.Instance.CursorController.SelectedAbility != null))
		{
			m_VisibleTrigger.Value = trigger;
		}
	}

	private void OnInspectUnit()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(base.ViewModel.HighlightedUnit.Value.EntityData);
		});
	}

	private void OnVisibleChanged(bool visible)
	{
		if (visible)
		{
			m_Animator.AppearAnimation();
			UISounds.Instance.Sounds.ActionBar.ActionBarShow.Play();
			m_ContainersForMarkers.ForEach(delegate(GameObject c)
			{
				c.Or(null)?.SetActive(value: true);
			});
		}
		else
		{
			m_Animator.DisappearAnimation();
			UISounds.Instance.Sounds.ActionBar.ActionBarHide.Play();
			m_ContainersForMarkers.ForEach(delegate(GameObject c)
			{
				c.Or(null)?.SetActive(value: false);
			});
		}
	}

	public void OnHighlightedUnit(AbstractUnitEntityView unit)
	{
		m_IsInspectVisible.Value = unit != null && InspectUnitsHelper.IsInspectAllow(unit.EntityData);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TriggerVisibility(isTurnBased);
	}

	public void HandleTurnBasedModeResumed()
	{
		HandleTurnBasedModeSwitched(isTurnBased: true);
	}
}
