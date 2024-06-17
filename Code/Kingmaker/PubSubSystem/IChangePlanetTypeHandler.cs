using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IChangePlanetTypeHandler : ISubscriber<PlanetEntity>, ISubscriber
{
	void HandleChangePlanetType();
}
