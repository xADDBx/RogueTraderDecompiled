using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnlockValueHandler : ISubscriber
{
	void HandleFlagValue(BlueprintUnlockableFlag flag, int value);
}
