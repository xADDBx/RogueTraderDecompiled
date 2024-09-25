using Kingmaker.Blueprints.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IToCargoAutomaticallyChangedHandler : ISubscriber
{
	void HandleToCargoAutomaticallyChanged(BlueprintItem blueprintItem);
}
