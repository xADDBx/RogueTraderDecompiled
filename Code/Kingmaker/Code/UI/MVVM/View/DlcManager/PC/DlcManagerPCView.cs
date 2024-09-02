using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Base;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.PC;
using Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.PC;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.PC;

public class DlcManagerPCView : DlcManagerBaseView
{
	[Header("Views")]
	[SerializeField]
	private DlcManagerTabDlcsPCView m_DlcManagerTabDlcsPCView;

	[SerializeField]
	private DlcManagerTabModsPCView m_DlcManagerTabModsPCView;

	[SerializeField]
	private DlcManagerTabSwitchOnDlcsPCView m_DlcManagerTabSwitchOnDlcsPCView;

	[SerializeField]
	private RectTransform m_BottomButtonsContainer;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_ApplyBottomButton;

	[SerializeField]
	private TextMeshProUGUI m_ApplyBottomButtonLabel;

	[SerializeField]
	private OwlcatButton m_DefaultBottomButton;

	[SerializeField]
	private TextMeshProUGUI m_DefaultBottomButtonLabel;

	protected override void InitializeImpl()
	{
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsPCView.Initialize();
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsPCView.Initialize();
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsPCView.Initialize();
		}
		m_ApplyBottomButton.SetInteractable(state: false);
		m_DefaultBottomButton.SetInteractable(state: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClose();
		}));
		AddDisposable(base.ViewModel.IsModsWindow.CombineLatest(base.ViewModel.IsSwitchOnDlcsWindow, (bool isModsWindow, bool isSwitchOnDlcsWindow) => new { isModsWindow, isSwitchOnDlcsWindow }).Subscribe(value =>
		{
			m_BottomButtonsContainer.gameObject.SetActive(value.isModsWindow || value.isSwitchOnDlcsWindow);
		}));
		SetButtonsSounds();
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsPCView.Bind(base.ViewModel.DlcsVM);
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsPCView.Bind(base.ViewModel.SwitchOnDlcsVM);
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsPCView.Bind(base.ViewModel.ModsVM);
		}
		m_ApplyBottomButtonLabel.text = UIStrings.Instance.SettingsUI.Apply;
		m_DefaultBottomButtonLabel.text = UIStrings.Instance.SettingsUI.Default;
		AddDisposable(m_ApplyBottomButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.CheckToReloadGame(null);
		}));
		AddDisposable(m_DefaultBottomButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.RestoreAllToPreviousState();
		}));
		if (base.ViewModel.InGame)
		{
			AddDisposable(base.ViewModel.ModsVM.NeedReload.CombineLatest(base.ViewModel.SwitchOnDlcsVM.NeedResave, (bool needReload, bool needResave) => new { needReload, needResave }).Subscribe(value =>
			{
				m_ApplyBottomButton.SetInteractable(value.needReload || value.needResave);
				m_DefaultBottomButton.SetInteractable(value.needReload || value.needResave);
			}));
		}
		else
		{
			AddDisposable(base.ViewModel.ModsVM.NeedReload.Subscribe(delegate(bool value)
			{
				m_ApplyBottomButton.SetInteractable(value);
				m_DefaultBottomButton.SetInteractable(value);
			}));
		}
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, m_Selector.OnPrev));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, m_Selector.OnNext));
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ApplyBottomButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_DefaultBottomButton, UISounds.ButtonSoundsEnum.PlastickSound);
	}
}
