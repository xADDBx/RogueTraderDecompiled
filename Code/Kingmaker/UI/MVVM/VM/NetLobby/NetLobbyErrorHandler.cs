using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;

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
		LobbyFullError,
		JoinLobbyError,
		CreatingLobbyError,
		PhotonCustomAuthenticationFailedError,
		SaveSourceDisconnectedError,
		SaveReceiveError,
		SaveNotFoundError,
		SendMessageFailError,
		NoPlaystationPlusError,
		UnknownException
	}

	private static UINetLobbyErrorsTexts ErrorsTexts => UIStrings.Instance.NetLobbyErrorsTexts;

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
		UINetUtility.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.StoreNotInitializedError));
	}

	public void HandleGetAuthDataTimeout()
	{
		UINetUtility.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.GetAuthDataTimeout));
	}

	public void HandleGetAuthDataError(string errorMessage)
	{
		UINetUtility.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.GetAuthDataError) + " " + errorMessage);
	}

	public void HandleChangeRegionError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.ChangeRegionError));
	}

	public void HandleNoPlayStationPlusError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.NoPlaystationPlusError));
	}

	public void HandleLobbyNotFoundError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.LobbyNotFoundError));
	}

	public void HandleLobbyFullError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.LobbyFullError));
	}

	public void HandleJoinLobbyError(int returnCode)
	{
		Show($"{ErrorsTexts.GetErrorMessage(NetLobbyErrorType.JoinLobbyError)} [{returnCode}]");
	}

	public void HandleCreatingLobbyError(short returnCode)
	{
		Show($"{ErrorsTexts.GetErrorMessage(NetLobbyErrorType.CreatingLobbyError)} [{returnCode}]");
	}

	public void HandlePhotonCustomAuthenticationFailedError()
	{
		UINetUtility.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.PhotonCustomAuthenticationFailedError));
	}

	public void HandleUnknownException()
	{
		UINetUtility.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.UnknownException));
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
}
