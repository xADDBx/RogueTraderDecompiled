using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetGameStartHandler : ISubscriber
{
	void HandleStartGameFailed();
}
