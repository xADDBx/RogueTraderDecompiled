using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnlockHandler : ISubscriber
{
	void HandleUnlock(BlueprintUnlockableFlag flag);

	void HandleLock(BlueprintUnlockableFlag flag);
}
