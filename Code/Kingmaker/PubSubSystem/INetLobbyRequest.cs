using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetLobbyRequest : ISubscriber
{
	void HandleNetLobbyRequest(bool isMainMenu = false);

	void HandleNetLobbyClose();
}
