using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetCheckUsersModsHandler : ISubscriber
{
	void HandleCheckUsersMods();
}
