using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.View.Common.InputField;
using Kingmaker.Code.UI.MVVM.View.MessageBox.Console;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.NetLobby.Console;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.EscMenu.Console;

public class EscMenuConsoleView : EscMenuBaseView, INetInviteHandler, ISubscriber
{
	[Header("Console Buttons")]
	[SerializeField]
	private OwlcatButton m_QuickSave;

	[SerializeField]
	private OwlcatButton m_QuickLoad;

	[Header("Console Buttons Labels")]
	[SerializeField]
	private TextMeshProUGUI m_QuickSaveButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_QuickLoadButtonLabel;

	protected override void BindViewImplementation()
	{
		AddDisposable(m_QuickSave.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnQuickSave));
		AddDisposable(m_QuickLoad.OnConfirmClickAsObservable().Subscribe(OnQuickLoadConfirm));
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged));
		m_QuickSave.SetInteractable(base.ViewModel.IsSavingAllowed);
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void UpdateInteractableButtonsImpl()
	{
		base.UpdateInteractableButtonsImpl();
		m_QuickSave.SetInteractable(base.ViewModel.IsSavingAllowed);
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		base.BuildNavigationImpl(navigationBehaviour);
		navigationBehaviour.InsertVertical(0, m_QuickSave);
		navigationBehaviour.InsertVertical(1, m_QuickLoad);
		navigationBehaviour.FocusOnFirstValidEntity();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		base.CreateInputImpl(inputLayer);
		AddDisposable(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 16, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			if (!Input.GetKey(KeyCode.Escape))
			{
				base.ViewModel.OnClose();
			}
		}, 9));
	}

	protected override void SetButtonsTexts()
	{
		base.SetButtonsTexts();
		m_QuickSaveButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuQuickSave;
		m_QuickLoadButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuQuickLoad;
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != InputLayer && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == "BugReportDuplicatesViewInput") && !(instance.CurrentInputLayer.ContextName == OwlcatDropdown.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatInputField.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == NetLobbyConsoleView.InputLayerName))
		{
			instance.PopLayer(InputLayer);
			instance.PushLayer(InputLayer);
		}
	}

	private void OnQuickLoadConfirm()
	{
		UIUtility.ShowMessageBox(UIStrings.Instance.SaveLoadTexts.ConfirmQuickLoad, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				base.ViewModel.OnQuickLoad();
			}
		});
	}

	public void HandleInvite(Action<bool> callback)
	{
	}

	public void HandleInviteAccepted(bool accepted)
	{
		PFLog.Net.Log($"ACCEPTED INVITE: {accepted}");
		if (accepted)
		{
			base.ViewModel.OnClose();
		}
	}
}
