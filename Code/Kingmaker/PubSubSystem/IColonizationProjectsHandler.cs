using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationProjectsHandler : ISubscriber
{
	void HandleColonyProjectStarted(Colony colony, ColonyProject project);

	void HandleColonyProjectFinished(Colony colony, ColonyProject project);
}
