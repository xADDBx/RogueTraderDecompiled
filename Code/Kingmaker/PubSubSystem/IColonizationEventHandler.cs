using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationEventHandler : ISubscriber
{
	void HandleEventStarted(Colony colony, BlueprintColonyEvent colonyEvent);

	void HandleEventFinished(Colony colony, BlueprintColonyEvent colonyEvent, BlueprintColonyEventResult result);
}
