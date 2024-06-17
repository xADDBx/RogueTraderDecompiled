using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDetachUnitsUIHandler : ISubscriber
{
	void HandleDetachUnits(int maxUnitInParty, ActionList afterDetach);
}
