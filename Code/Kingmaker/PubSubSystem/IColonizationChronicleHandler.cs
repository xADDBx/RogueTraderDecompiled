using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationChronicleHandler : ISubscriber
{
	void HandleChronicleStarted(Colony colony, BlueprintDialog chronicle);

	void HandleChronicleFinished(Colony colony, ColonyChronicle chronicle);
}
