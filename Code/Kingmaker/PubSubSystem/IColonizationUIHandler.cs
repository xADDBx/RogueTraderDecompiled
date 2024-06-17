using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationUIHandler : ISubscriber
{
	void UpdateBuildingScreen(BlueprintColonyProject project, Colony colony);
}
