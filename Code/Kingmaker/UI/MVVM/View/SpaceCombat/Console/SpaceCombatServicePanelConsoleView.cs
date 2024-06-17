using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class SpaceCombatServicePanelConsoleView : ViewBase<SpaceCombatServicePanelVM>
{
	[Header("Ship Health")]
	[SerializeField]
	private DelayedSlider m_HPSlider;

	[SerializeField]
	private TextMeshProUGUI m_ShipHealth;

	[Header("Turn Counter")]
	[SerializeField]
	private GameObject m_TurnCounter;

	[SerializeField]
	private TextMeshProUGUI m_RoundsLeft;

	[Header("Initiative Tracker")]
	[SerializeField]
	private InitiativeTrackerVerticalConsoleView m_InitiativeTrackerView;

	[SerializeField]
	private SpaceCombatTorpedoPanelConsoleView m_SpaceCombatTorpedoPanelConsoleView;

	[Header("Combat HUD")]
	[SerializeField]
	private MoveAnimator m_ControlPanelAnimator;

	[SerializeField]
	private MoveAnimator m_TorpedoesPanelAnimator;

	[Header("Input")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_CoopRolesHint;

	[SerializeField]
	private Image m_NetRolesAttentionMark;

	[SerializeField]
	private ConsoleHint m_EscMenuHint;

	private IDisposable m_TurnUnitSubscription;

	private IDisposable m_SelectedUnitSubscription;

	private IDisposable m_Hint;

	private readonly BoolReactiveProperty m_HintsEnabled = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_NetFirstLoadState = new BoolReactiveProperty();

	public void Initialize()
	{
		m_HPSlider.Initialize();
		m_InitiativeTrackerView.Initialize();
		m_SpaceCombatTorpedoPanelConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShipHealthRatio.Subscribe(delegate(float value)
		{
			m_HPSlider.SetValue(value);
		}));
		AddDisposable(base.ViewModel.ShipHealthText.Subscribe(delegate(string value)
		{
			m_ShipHealth.text = value;
		}));
		AddDisposable(base.ViewModel.IsInTimeSurvival.Subscribe(delegate(bool value)
		{
			m_TurnCounter.SetActive(value);
		}));
		AddDisposable(base.ViewModel.RoundsLeft.Subscribe(delegate(string value)
		{
			m_RoundsLeft.text = value;
		}));
		AddDisposable(m_RoundsLeft.SetHint(UIStrings.Instance.SpaceCombatTexts.TimeSurvivalHint));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.IsPlayerTurn).Subscribe(ShowControlPanelHUD));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.IsTorpedoesTurn).Subscribe(ShowTorpedoesHUD));
		AddDisposable(base.ViewModel.IsPlayerTurn.CombineLatest(base.ViewModel.IsTorpedoesTurn, (bool isPlayerTurn, bool isTorpedoesTurn) => isPlayerTurn || isTorpedoesTurn).Subscribe(delegate(bool value)
		{
			m_HintsEnabled.Value = value;
		}));
		AddDisposable(base.ViewModel.NetFirstLoadState.Subscribe(delegate(bool value)
		{
			m_NetFirstLoadState.Value = value;
		}));
		AddDisposable(base.ViewModel.InitiativeTrackerVM.Subscribe(delegate(InitiativeTrackerVM tracker)
		{
			m_InitiativeTrackerView.Bind(tracker);
			if (tracker != null)
			{
				m_SelectedUnitSubscription?.Dispose();
				m_SelectedUnitSubscription = null;
				m_TurnUnitSubscription = tracker.CurrentUnit.Subscribe(m_SpaceCombatTorpedoPanelConsoleView.Bind);
				AddDisposable(m_TurnUnitSubscription);
			}
			else
			{
				m_TurnUnitSubscription?.Dispose();
				m_TurnUnitSubscription = null;
				m_SelectedUnitSubscription = base.ViewModel.SelectedUnitVM.Subscribe(m_SpaceCombatTorpedoPanelConsoleView.Bind);
				AddDisposable(m_SelectedUnitSubscription);
			}
		}));
		AddDisposable(base.ViewModel.PlayerHaveRoles.CombineLatest(base.ViewModel.NetFirstLoadState, (bool haveRoles, bool netFirstLoadState) => new { haveRoles, netFirstLoadState }).Subscribe(value =>
		{
			m_NetRolesAttentionMark.gameObject.SetActive(value.netFirstLoadState && !value.haveRoles);
		}));
	}

	public void AddCombatInput(InputLayer inputLayer)
	{
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(CombatLogChangeState, 15, m_HintsEnabled), UIStrings.Instance.HUDTexts.EnterCombatLog, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(EndTurn, 17, m_HintsEnabled), UIStrings.Instance.HUDTexts.EndTurn, ConsoleHintsWidget.HintPosition.Left));
		m_InitiativeTrackerView.AddInput(inputLayer);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(FocusOnCurrentUnit, 19, m_HintsEnabled, InputActionEventType.ButtonJustReleased), UIStrings.Instance.HUDTexts.FocusOnCurrentUnit, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_CoopRolesHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OpenNetRoles();
		}, 18, m_NetFirstLoadState, InputActionEventType.ButtonJustLongPressed)));
		m_CoopRolesHint.SetLabel(UIStrings.Instance.EscapeMenu.EscMenuRoles);
		AddDisposable(m_EscMenuHint.Bind(inputLayer.AddButton(delegate
		{
		}, 16, InputActionEventType.ButtonJustReleased)));
		m_EscMenuHint.SetLabel(UIStrings.Instance.MainMenu.Settings);
	}

	private void CombatLogChangeState(InputActionEventData obj)
	{
		EventBus.RaiseEvent(delegate(ICombatLogChangeStateHandler h)
		{
			h.CombatLogChangeState();
		});
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void ShowTorpedoesHUD(bool val)
	{
		if (val)
		{
			m_TorpedoesPanelAnimator.AppearAnimation();
		}
		else
		{
			m_TorpedoesPanelAnimator.DisappearAnimation();
		}
	}

	private void ShowControlPanelHUD(bool val)
	{
		if (val)
		{
			m_ControlPanelAnimator.AppearAnimation();
		}
		else
		{
			m_ControlPanelAnimator.DisappearAnimation();
		}
	}

	private void EndTurn(InputActionEventData eventData)
	{
		base.ViewModel.EndTurn();
	}

	private void FocusOnCurrentUnit(InputActionEventData data)
	{
		TurnController turnController = Game.Instance.TurnController;
		if (turnController.IsPlayerTurn)
		{
			Game.Instance.CameraController?.Follower?.ScrollTo(turnController.CurrentUnit);
		}
	}
}
