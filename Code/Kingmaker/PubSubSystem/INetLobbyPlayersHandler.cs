using Kingmaker.PubSubSystem.Core.Interfaces;
using Photon.Realtime;

namespace Kingmaker.PubSubSystem;

public interface INetLobbyPlayersHandler : ISubscriber
{
	void HandlePlayerEnteredRoom(Photon.Realtime.Player player);

	void HandlePlayerLeftRoom(Photon.Realtime.Player player);

	void HandlePlayerChanged();

	void HandleLastPlayerLeftLobby();

	void HandleRoomOwnerChanged();
}
