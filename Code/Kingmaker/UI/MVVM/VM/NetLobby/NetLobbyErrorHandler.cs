using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Photon.Realtime;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyErrorHandler : INetLobbyErrorHandler, ISubscriber, INetGameStartHandler, IDisposable
{
	public enum NetLobbyErrorType
	{
		StoreNotInitializedError,
		GetAuthDataTimeout,
		GetAuthDataError,
		ChangeRegionError,
		LobbyNotFoundError,
		JoinLobbyError,
		CreatingLobbyError,
		PhotonDisconnectedError,
		PhotonCustomAuthenticationFailedError,
		SaveSourceDisconnectedError,
		SaveReceiveError,
		SaveNotFoundError,
		SendMessageFailError,
		UnknownException
	}

	private static UINetLobbyErrorsTexts ErrorsTexts => UIStrings.Instance.NetLobbyErrorsTexts;

	private static string ReconnectLabel => UIStrings.Instance.NetLobbyTexts.Reconnect;

	public NetLobbyErrorHandler()
	{
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleStoreNotInitializedError()
	{
		ShowReconnectable(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.StoreNotInitializedError));
	}

	public void HandleGetAuthDataTimeout()
	{
		ShowReconnectable(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.GetAuthDataTimeout));
	}

	public void HandleGetAuthDataError(string errorMessage)
	{
		ShowReconnectable(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.GetAuthDataError) + " " + errorMessage);
	}

	public void HandleChangeRegionError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.ChangeRegionError));
	}

	public void HandleLobbyNotFoundError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.LobbyNotFoundError));
	}

	public void HandleJoinLobbyError(short returnCode)
	{
		Show($"{ErrorsTexts.GetErrorMessage(NetLobbyErrorType.JoinLobbyError)} [{returnCode}]");
	}

	public void HandleCreatingLobbyError(short returnCode)
	{
		Show($"{ErrorsTexts.GetErrorMessage(NetLobbyErrorType.CreatingLobbyError)} [{returnCode}]");
	}

	public void HandlePhotonDisconnectedError(DisconnectCause cause, bool allowReconnect)
	{
		string text = ErrorsTexts.GetErrorMessage(NetLobbyErrorType.PhotonDisconnectedError) + " " + ReasonStrings.Instance.GetDisconnectCause(cause);
		if (allowReconnect)
		{
			ShowReconnectable(text);
		}
		else
		{
			Show(text, CloseLobby);
		}
	}

	public void HandlePhotonCustomAuthenticationFailedError()
	{
		ShowReconnectable(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.PhotonCustomAuthenticationFailedError));
	}

	public void HandleUnknownException()
	{
		ShowReconnectable(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.UnknownException));
	}

	public void HandleStartGameFailed()
	{
		Show(UIStrings.Instance.NetLobbyTexts.IsNotEnoughPlayersForGame);
	}

	private static void Show(string text, Action onCloseAction = null, string yesLabel = null)
	{
		UIUtility.ShowMessageBox(text, DialogMessageBoxBase.BoxType.Message, delegate
		{
			onCloseAction?.Invoke();
		}, null, yesLabel);
	}

	private static void ShowReconnectable(string text)
	{
		UIUtility.ShowMessageBox(text, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton btn)
		{
			if (btn == DialogMessageBoxBase.BoxButton.Yes)
			{
				PhotonManager.NetGame.StartNetGameIfNeeded();
			}
			else
			{
				CloseLobby();
			}
		}, null, ReconnectLabel, UIStrings.Instance.CommonTexts.Cancel);
	}

	private static void CloseLobby()
	{
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyClose();
		});
	}
}
