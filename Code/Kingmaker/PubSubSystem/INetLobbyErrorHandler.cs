using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetLobbyErrorHandler : ISubscriber
{
	void HandleStoreNotInitializedError();

	void HandleGetAuthDataTimeout();

	void HandleGetAuthDataError();

	void HandleChangeRegionError();

	void HandleNoPlayStationPlusError();

	void HandleUserPermissionsError(bool reconnectDialog = false);

	void HandleLobbyNotFoundError();

	void HandleLobbyFullError();

	void HandleJoinLobbyError(int returnCode);

	void HandleCreatingLobbyError(short returnCode);

	void HandlePhotonCustomAuthenticationFailedError();

	void HandleUnknownException();
}
