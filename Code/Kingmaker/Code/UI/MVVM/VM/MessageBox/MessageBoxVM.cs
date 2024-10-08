using System;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.MessageBox;

public class MessageBoxVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetLobbyRequest, ISubscriber
{
	public readonly DialogMessageBoxBase.BoxType BoxType;

	public readonly string MessageText;

	public readonly string AcceptText;

	public readonly string DeclineText;

	public readonly string InputPlaceholder;

	public readonly BoolReactiveProperty ShowDecline = new BoolReactiveProperty();

	public readonly ReactiveProperty<string> InputText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<int> WaitTime = new ReactiveProperty<int>(0);

	public readonly BoolReactiveProperty IsProgressBar = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsCheckbox = new BoolReactiveProperty();

	private readonly Action<DialogMessageBoxBase.BoxButton> m_OnClose;

	private readonly Action<string> m_TextClose;

	private readonly Action<TMP_LinkInfo> m_LinkInvoke;

	private readonly Action m_DisposeAction;

	public readonly FloatReactiveProperty LoadingProgress;

	public readonly ReactiveCommand LoadingProgressCloseTrigger;

	private readonly Action m_DontShowAgainAction;

	public MessageBoxVM(string messageText, DialogMessageBoxBase.BoxType boxType, Action<DialogMessageBoxBase.BoxButton> onClose, Action<TMP_LinkInfo> onLinkInvoke, string yesLabel, string noLabel, Action<string> onTextClose, string inputText, string inputPlaceholder, int waitTime, Action disposeAction, FloatReactiveProperty loadingProgress, ReactiveCommand loadingProgressCloseTrigger, Action dontShowAgainAction)
	{
		AddDisposable(EventBus.Subscribe(this));
		BoxType = boxType;
		UICommonTexts commonTexts = UIStrings.Instance.CommonTexts;
		MessageText = messageText;
		AcceptText = (string.IsNullOrEmpty(yesLabel) ? ((string)commonTexts.Accept) : yesLabel);
		DeclineText = (string.IsNullOrEmpty(noLabel) ? ((string)commonTexts.Cancel) : noLabel);
		IsProgressBar.Value = boxType == DialogMessageBoxBase.BoxType.ProgressBar;
		IsCheckbox.Value = boxType == DialogMessageBoxBase.BoxType.Checkbox && !IsProgressBar.Value;
		ShowDecline.Value = boxType != 0 && !IsProgressBar.Value;
		m_OnClose = onClose;
		m_TextClose = onTextClose;
		m_LinkInvoke = onLinkInvoke;
		InputText.Value = (string.IsNullOrEmpty(inputText) ? string.Empty : inputText);
		InputPlaceholder = (string.IsNullOrEmpty(inputPlaceholder) ? string.Empty : inputPlaceholder);
		WaitTime.Value = waitTime;
		m_DisposeAction = disposeAction;
		DOTween.To(() => WaitTime.Value, delegate(int value)
		{
			WaitTime.Value = value;
		}, 0, waitTime).SetUpdate(isIndependentUpdate: true);
		LoadingProgress = loadingProgress;
		LoadingProgressCloseTrigger = loadingProgressCloseTrigger;
		m_DontShowAgainAction = dontShowAgainAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnAcceptPressed()
	{
		m_OnClose?.Invoke(DialogMessageBoxBase.BoxButton.Yes);
		m_TextClose?.Invoke(InputText.Value);
		m_DisposeAction?.Invoke();
	}

	public void OnDeclinePressed()
	{
		m_OnClose?.Invoke(DialogMessageBoxBase.BoxButton.No);
		m_TextClose?.Invoke(string.Empty);
		m_DisposeAction?.Invoke();
	}

	public void OnLinkInvoke(TMP_LinkInfo linkInfo)
	{
		m_LinkInvoke?.Invoke(linkInfo);
	}

	public void DontShowAgainInvoke()
	{
		if (IsCheckbox.Value && m_DontShowAgainAction != null)
		{
			m_DontShowAgainAction?.Invoke();
		}
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		if (!IsProgressBar.Value)
		{
			OnDeclinePressed();
		}
	}

	public void HandleNetLobbyClose()
	{
	}
}
