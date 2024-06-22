using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.MVVM.View.CharGen.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.PC;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.PC;

public class CharGenPCView : CharGenView
{
	[SerializeField]
	private OwlcatButton m_VisualSettingsViewButton;

	[SerializeField]
	private CharacterVisualSettingsPCView m_VisualSettingsPCView;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_NextButton;

	[SerializeField]
	private TextMeshProUGUI m_NextButtonLabel;

	[SerializeField]
	private OwlcatButton m_BackButton;

	[SerializeField]
	private TextMeshProUGUI m_BackButtonLabel;

	private readonly StringReactiveProperty m_NextButtonHint = new StringReactiveProperty(string.Empty);

	private readonly CompositeDisposable m_CurrentPhaseDisposable = new CompositeDisposable();

	private bool m_IsShowed;

	public override void Initialize()
	{
		base.Initialize();
		m_VisualSettingsPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetButtonsSounds();
		AddDisposable(base.ViewModel.VisualSettingsVM.Subscribe(delegate(CharacterVisualSettingsVM vm)
		{
			m_VisualSettingsPCView.Bind(vm);
			m_VisualSettingsViewButton.gameObject.SetActive(vm == null);
		}));
		AddDisposable(m_VisualSettingsViewButton.SetHint(UIStrings.Instance.CharGen.ShowVisualSettings));
		AddDisposable(m_VisualSettingsViewButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowVisualSettings();
		}));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CloseCharGen();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(CloseCharGen));
		AddDisposable(m_NextButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			NextPressed();
		}));
		AddDisposable(m_BackButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			BackPressed();
		}));
		AddDisposable(base.ViewModel.CurrentPhaseVM.Subscribe(delegate(CharGenPhaseBaseVM phase)
		{
			m_NextButtonLabel.text = (base.CurrentPhaseIsLast ? UIStrings.Instance.CharGen.Complete : UIStrings.Instance.CharGen.Next);
			m_CurrentPhaseDisposable.Clear();
			m_CurrentPhaseDisposable.Add(phase.PhaseNextHint.Subscribe(delegate(string value)
			{
				m_NextButtonHint.Value = value;
			}));
		}));
		AddDisposable(CanGoNext.Subscribe(SetActiveNextPhaseButton));
		AddDisposable(CanGoBack.Subscribe(SetActiveBackPhaseButton));
		AddDisposable(m_NextButton.SetHint(m_NextButtonHint));
		m_NextButtonLabel.text = UIStrings.Instance.CharGen.Next;
		m_BackButtonLabel.text = UIStrings.Instance.CharGen.Back;
		CheckCoopButtons(base.ViewModel.IsMainCharacter.Value);
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, BackPressed));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, NextPressed));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_CurrentPhaseDisposable.Dispose();
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_CloseButton.gameObject.SetActive(isMainCharacter);
		m_NextButton.gameObject.SetActive(isMainCharacter);
		m_BackButton.gameObject.SetActive(isMainCharacter);
		m_VisualSettingsViewButton.gameObject.SetActive(isMainCharacter);
	}

	private void SetActiveNextPhaseButton(bool active)
	{
		m_NextButton.Interactable = active;
	}

	private void SetActiveBackPhaseButton(bool active)
	{
		m_BackButton.Interactable = active;
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_VisualSettingsViewButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_NextButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_BackButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
	}

	protected override void CloseCharGen()
	{
		if (m_VisualSettingsPCView.IsBinded)
		{
			m_VisualSettingsPCView.Close();
		}
		else
		{
			base.CloseCharGen();
		}
	}
}
