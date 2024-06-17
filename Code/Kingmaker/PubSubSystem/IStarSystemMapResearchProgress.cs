using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IStarSystemMapResearchProgress : ISubscriber
{
	void HandleResearchPercentRecalculate(BlueprintStarSystemMap areaBlueprint, float value);
}
