using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUIMultiEntranceHandler : ISubscriber
{
	void HandleMultiEntranceUI(BlueprintMultiEntrance entrances);
}
