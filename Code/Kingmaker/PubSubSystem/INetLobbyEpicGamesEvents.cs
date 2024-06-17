using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetLobbyEpicGamesEvents : ISubscriber
{
	void HandleSetEpicGamesButtonActive(bool state);

	void HandleSetEpicGamesUserName(bool isAuthorized, string name);
}
