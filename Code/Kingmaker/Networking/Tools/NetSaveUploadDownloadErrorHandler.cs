using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.NetLobby;

namespace Kingmaker.Networking.Tools;

public class NetSaveUploadDownloadErrorHandler : INetSaveUploadDownloadErrorHandler, ISubscriber
{
	private UINetLobbyErrorsTexts ErrorsTexts => UIStrings.Instance.NetLobbyErrorsTexts;

	public NetSaveUploadDownloadErrorHandler()
	{
		EventBus.Subscribe(this);
	}

	public void HandleSaveSourceDisconnectedError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorHandler.NetLobbyErrorType.SaveSourceDisconnectedError));
	}

	public void HandleSaveReceiveError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorHandler.NetLobbyErrorType.SaveReceiveError));
	}

	public void HandleSaveNotFoundError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorHandler.NetLobbyErrorType.SaveNotFoundError));
	}

	public void HandleSendMessageFailError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorHandler.NetLobbyErrorType.SendMessageFailError));
	}

	public void HandleUnknownException()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorHandler.NetLobbyErrorType.UnknownException));
	}

	private void Show(string text)
	{
		UIUtility.ShowMessageBox(text, DialogMessageBoxBase.BoxType.Message, delegate
		{
		});
	}
}
