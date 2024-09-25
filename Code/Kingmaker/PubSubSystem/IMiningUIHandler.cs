using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IMiningUIHandler : ISubscriber
{
	void HandleStartMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource);

	void HandleStopMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource);
}
