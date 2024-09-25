using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAnomalyUIHandler : ISubscriber
{
	void OpenAnomalyScreen(AnomalyView anomalyObject);

	void UpdateAnomalyScreen(AnomalyView anomalyObject);

	void CloseAnomalyScreen();
}
