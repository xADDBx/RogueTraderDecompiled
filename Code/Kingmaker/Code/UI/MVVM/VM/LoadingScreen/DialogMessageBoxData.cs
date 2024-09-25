using System;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using TMPro;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.LoadingScreen;

public class DialogMessageBoxData
{
	public string MessageText { get; }

	public DialogMessageBoxBase.BoxType BoxType { get; }

	public Action<DialogMessageBoxBase.BoxButton> OnClose { get; }

	public Action<TMP_LinkInfo> OnLinkInvoke { get; }

	public string YesLabel { get; }

	public string NoLabel { get; }

	public Action<string> OnTextResult { get; }

	public string InputText { get; }

	public string InputPlaceholder { get; }

	public int WaitTime { get; }

	public uint MaxInputTextLength { get; }

	public FloatReactiveProperty LoadingProgress { get; }

	public ReactiveCommand LoadingProgressCloseTrigger { get; }

	public Action DontShowAgainAction { get; }

	public DialogMessageBoxData(string messageText, DialogMessageBoxBase.BoxType boxType = DialogMessageBoxBase.BoxType.Message, Action<DialogMessageBoxBase.BoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue, FloatReactiveProperty loadingProgress = null, ReactiveCommand loadingProgressCloseTrigger = null, Action dontShowAgainAction = null)
	{
		MessageText = messageText;
		BoxType = boxType;
		OnClose = onClose;
		OnLinkInvoke = onLinkInvoke;
		YesLabel = yesLabel;
		NoLabel = noLabel;
		OnTextResult = onTextResult;
		InputText = inputText;
		InputPlaceholder = inputPlaceholder;
		WaitTime = waitTime;
		MaxInputTextLength = maxInputTextLength;
		LoadingProgress = loadingProgress;
		LoadingProgressCloseTrigger = loadingProgressCloseTrigger;
		DontShowAgainAction = dontShowAgainAction;
	}
}
