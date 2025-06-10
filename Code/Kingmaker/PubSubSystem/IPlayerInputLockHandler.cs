using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPlayerInputLockHandler : ISubscriber
{
	void HandlePlayerInputLocked();

	void HandlePlayerInputUnlocked();
}
