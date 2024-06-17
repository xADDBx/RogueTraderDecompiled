using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationProjectsUIHandler : ISubscriber
{
	void HandleColonyProjectsUIOpen(Colony colony);

	void HandleColonyProjectsUIClose();

	void HandleColonyProjectPage(BlueprintColonyProject blueprintColonyProject);
}
