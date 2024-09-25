using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationResourcesHandler : ISubscriber
{
	void HandleColonyResourcesUpdated(BlueprintResource resource, int count);

	void HandleNotFromColonyResourcesUpdated(BlueprintResource resource, int count);
}
