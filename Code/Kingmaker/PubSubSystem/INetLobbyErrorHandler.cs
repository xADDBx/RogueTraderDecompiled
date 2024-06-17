using Kingmaker.PubSubSystem.Core.Interfaces;
using Photon.Realtime;

namespace Kingmaker.PubSubSystem;

public interface INetLobbyErrorHandler : ISubscriber
{
	void HandleStoreNotInitializedError();

	void HandleGetAuthDataTimeout();

	void HandleGetAuthDataError(string errorMessage);

	void HandleChangeRegionError();

	void HandleLobbyNotFoundError();

	void HandleJoinLobbyError(short returnCode);

	void HandleCreatingLobbyError(short returnCode);

	void HandlePhotonDisconnectedError(DisconnectCause cause, bool allowReconnect);

	void HandlePhotonCustomAuthenticationFailedError();

	void HandleUnknownException();
}
