using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationHandler : ISubscriber
{
	void HandleColonyCreated(Colony colony, PlanetEntity planetEntity);
}
