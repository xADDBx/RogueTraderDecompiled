using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IOpenExplorationScreenAfterColonization : ISubscriber
{
	void HandleTryOpenExplorationScreenAfterColonization(PlanetEntity planet);
}
