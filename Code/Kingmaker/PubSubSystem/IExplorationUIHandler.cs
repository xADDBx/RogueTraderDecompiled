using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

public interface IExplorationUIHandler : ISubscriber
{
	void OpenExplorationScreen(MapObjectView explorationObjectView);

	void CloseExplorationScreen();
}
