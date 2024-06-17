using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IStartAwaitingUserInput : ISubscriber
{
	void OnStartAwaitingUserInput();
}
