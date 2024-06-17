using Kingmaker.Inspect;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IKnowledgeHandler : ISubscriber
{
	void HandleKnowledgeUpdated(InspectUnitsManager.UnitInfo unitInfo);
}
