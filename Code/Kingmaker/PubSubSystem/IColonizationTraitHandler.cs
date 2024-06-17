using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationTraitHandler : ISubscriber
{
	void HandleTraitStarted(Colony colony, BlueprintColonyTrait trait);

	void HandleTraitEnded(Colony colony, BlueprintColonyTrait trait);
}
