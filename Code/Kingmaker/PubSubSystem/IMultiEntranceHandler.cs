using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IMultiEntranceHandler : ISubscriber
{
	void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance);
}
