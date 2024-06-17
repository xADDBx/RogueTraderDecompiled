using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IAreaLoadingStagesHandler : ISubscriber
{
	void OnAreaScenesLoaded();

	void OnAreaLoadingComplete();
}
