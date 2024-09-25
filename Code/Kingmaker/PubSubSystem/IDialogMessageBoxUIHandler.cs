using System;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem.Core.Interfaces;
using TMPro;
using UniRx;

namespace Kingmaker.PubSubSystem;

public interface IDialogMessageBoxUIHandler : ISubscriber
{
	void HandleOpen(string messageText, DialogMessageBoxBase.BoxType boxType = DialogMessageBoxBase.BoxType.Message, Action<DialogMessageBoxBase.BoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue, FloatReactiveProperty loadingProgress = null, ReactiveCommand loadingProgressCloseTrigger = null, Action dontShowAgainAction = null);

	void HandleClose();
}
