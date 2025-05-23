using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.ChangeAppearance.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.PC;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ChangeAppearance.PC;

public class ChangeAppearancePCView : ChangeAppearanceView
{
	[SerializeField]
	private OwlcatButton m_VisualSettingsViewButton;

	[SerializeField]
	private CharacterVisualSettingsPCView m_VisualSettingsPCView;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CancelButton;

	[SerializeField]
	private TextMeshProUGUI m_CancelButtonLabel;

	[SerializeField]
	private OwlcatButton m_ConfirmButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmButtonLabel;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private RectTransform m_BottomButtonsContainer;

	private bool m_IsShowed;

	public override void Initialize()
	{
		base.Initialize();
		m_VisualSettingsPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnClose();
		}));
		AddDisposable(m_CancelButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnClose();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(OnClose));
		AddDisposable(m_ConfirmButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnConfirm();
		}));
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
		m_ConfirmButtonLabel.text = UIStrings.Instance.CommonTexts.Accept;
		m_CancelButtonLabel.text = UIStrings.Instance.CommonTexts.Cancel;
		CheckCoopButtons(base.ViewModel.IsMainCharacter.Value);
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons));
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_VisualSettingsViewButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ConfirmButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
	}

	protected override void OnClose()
	{
		if (m_VisualSettingsPCView.IsBinded)
		{
			m_VisualSettingsPCView.Close();
		}
		else
		{
			base.OnClose();
		}
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_CloseButton.Or(null)?.gameObject.Or(null)?.SetActive(isMainCharacter);
		m_ConfirmButton.Or(null)?.gameObject.Or(null)?.SetActive(isMainCharacter);
		m_VisualSettingsViewButton.Or(null)?.gameObject.Or(null)?.SetActive(isMainCharacter);
		m_BottomButtonsContainer.Or(null)?.gameObject.Or(null)?.SetActive(isMainCharacter);
	}
}
