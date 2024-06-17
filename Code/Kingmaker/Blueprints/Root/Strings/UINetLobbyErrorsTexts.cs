using System;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.NetLobby;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UINetLobbyErrorsTexts
{
	public LocalizedString StoreNotInitializedErrorMessage;

	public LocalizedString GetAuthDataTimeoutMessage;

	public LocalizedString GetAuthDataErrorMessage;

	public LocalizedString ChangeRegionErrorMessage;

	public LocalizedString LobbyNotFoundErrorMessage;

	public LocalizedString JoinLobbyErrorMessage;

	public LocalizedString CreatingLobbyErrorMessage;

	public LocalizedString PhotonDisconnectedErrorMessage;

	public LocalizedString PhotonCustomAuthenticationFailedErrorMessage;

	public LocalizedString SaveSourceDisconnectedErrorMessage;

	public LocalizedString SaveReceiveErrorMessage;

	public LocalizedString SaveNotFoundErrorMessage;

	public LocalizedString SendMessageFailErrorMessage;

	public LocalizedString UnknownExceptionMessage;

	public string GetErrorMessage(NetLobbyErrorHandler.NetLobbyErrorType type)
	{
		return type switch
		{
			NetLobbyErrorHandler.NetLobbyErrorType.StoreNotInitializedError => StoreNotInitializedErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.GetAuthDataTimeout => GetAuthDataTimeoutMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.GetAuthDataError => GetAuthDataErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.ChangeRegionError => ChangeRegionErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.LobbyNotFoundError => LobbyNotFoundErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.JoinLobbyError => JoinLobbyErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.CreatingLobbyError => CreatingLobbyErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.PhotonDisconnectedError => PhotonDisconnectedErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.PhotonCustomAuthenticationFailedError => PhotonCustomAuthenticationFailedErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.SaveSourceDisconnectedError => SaveSourceDisconnectedErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.SaveReceiveError => SaveReceiveErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.SaveNotFoundError => SaveNotFoundErrorMessage, 
			NetLobbyErrorHandler.NetLobbyErrorType.SendMessageFailError => SendMessageFailErrorMessage, 
			_ => UnknownExceptionMessage, 
		};
	}
}
