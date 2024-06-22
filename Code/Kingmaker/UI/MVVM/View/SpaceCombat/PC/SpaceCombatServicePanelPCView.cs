using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class SpaceCombatServicePanelPCView : ViewBase<SpaceCombatServicePanelVM>
{
	[Header("Ship Health")]
	[SerializeField]
	private DelayedSlider m_HPSlider;

	[SerializeField]
	private TextMeshProUGUI m_ShipHealth;

	[Header("End Turn Button")]
	[SerializeField]
	private OwlcatMultiButton m_EndTurnButton;

	[SerializeField]
	private MoveAnimator m_EndTurnButtonAnimator;

	[SerializeField]
	private TextMeshProUGUI m_EndTurnBindText;

	private SettingsEntityKeyBindingPair m_PauseBind;

	[Header("Turn Counter")]
	[SerializeField]
	private OwlcatMultiSelectable m_TurnCounter;

	[SerializeField]
	private TextMeshProUGUI m_RoundsLeft;

	[Header("Initiative Tracker")]
	[SerializeField]
	private InitiativeTrackerView m_InitiativeTrackerView;

	[SerializeField]
	private SpaceCombatTorpedoPanelPCView m_SpaceCombatTorpedoPanelPCView;

	[Header("Combat HUD")]
	[SerializeField]
	private MoveAnimator m_ControlPanelAnimator;

	[SerializeField]
	private MoveAnimator m_TorpedoesPanelAnimator;

	private IDisposable m_TurnUnitSubscription;

	private IDisposable m_SelectedUnitSubscription;

	private IDisposable m_Hint;

	public void Initialize()
	{
		m_HPSlider.Initialize();
		m_InitiativeTrackerView.Initialize();
		m_EndTurnButtonAnimator.Initialize();
		m_SpaceCombatTorpedoPanelPCView.Initialize();
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
			m_TurnCounter.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.RoundsLeft.Subscribe(delegate(string value)
		{
			m_RoundsLeft.text = value;
		}));
		AddDisposable(m_TurnCounter.SetHint(UIStrings.Instance.SpaceCombatTexts.TimeSurvivalHint));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.IsPlayerTurn.Or(base.ViewModel.IsTorpedoesTurn)).And(base.ViewModel.IsAvailable).Subscribe(ShowEndTurnButton));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.IsPlayerTurn).Subscribe(ShowControlPanelHUD));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.IsTorpedoesTurn).Subscribe(ShowTorpedoesHUD));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.CanEndTurn).Subscribe(delegate(bool val)
		{
			m_EndTurnButton.SetActiveLayer((!val) ? 1 : 0);
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_EndTurnButton.OnLeftClickAsObservable(), delegate
		{
			UISounds.Instance.Sounds.Combat.EndTurn.Play();
			base.ViewModel.EndTurn();
		}));
		AddDisposable(base.ViewModel.InitiativeTrackerVM.Subscribe(delegate(InitiativeTrackerVM tracker)
		{
			m_InitiativeTrackerView.Bind(tracker);
			if (tracker != null)
			{
				m_SelectedUnitSubscription?.Dispose();
				m_SelectedUnitSubscription = null;
				m_TurnUnitSubscription = tracker.CurrentUnit.Subscribe(m_SpaceCombatTorpedoPanelPCView.Bind);
				AddDisposable(m_TurnUnitSubscription);
			}
			else
			{
				m_TurnUnitSubscription?.Dispose();
				m_TurnUnitSubscription = null;
				m_SelectedUnitSubscription = base.ViewModel.SelectedUnitVM.Subscribe(m_SpaceCombatTorpedoPanelPCView.Bind);
				AddDisposable(m_SelectedUnitSubscription);
			}
		}));
		m_PauseBind = SettingsRoot.Controls.Keybindings.General.Pause;
		m_PauseBind.OnValueChanged += SetEndTurnBindText;
		SetEndTurnBindText();
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SpeedUpEnemiesTurn.name + UIConsts.SuffixOn, delegate
		{
			SpeedUp(state: true);
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SpeedUpEnemiesTurn.name + UIConsts.SuffixOff, delegate
		{
			SpeedUp(state: false);
		}));
	}

	private void SpeedUp(bool state)
	{
		Game.Instance.SpeedUp(state);
	}

	protected override void DestroyViewImplementation()
	{
		m_PauseBind.OnValueChanged -= SetEndTurnBindText;
		m_PauseBind = null;
	}

	private void ShowEndTurnButton(bool val)
	{
		if (val)
		{
			base.gameObject.SetActive(value: true);
			m_EndTurnButtonAnimator.AppearAnimation();
		}
		else
		{
			m_EndTurnButtonAnimator.DisappearAnimation();
		}
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

	private void SetEndTurnBindText(KeyBindingPair keyBindingPair = default(KeyBindingPair))
	{
		AddDisposable(m_EndTurnButton.SetHint(UIStrings.Instance.Tooltips.EndTurn, "EndTurn"));
		m_EndTurnBindText.text = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName("EndTurn"));
	}
}
