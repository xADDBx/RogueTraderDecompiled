using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEscMenuHandler : ISubscriber
{
	void HandleOpen();
}
