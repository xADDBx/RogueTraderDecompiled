using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAnomalyHandler : ISubscriber<AnomalyEntityData>, ISubscriber
{
	void HandleAnomalyInteracted();
}
