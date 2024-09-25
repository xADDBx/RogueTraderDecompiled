using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Warhammer.SpaceCombat;

public interface ITimeSurvivalSpawnHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleStarshipSpawnStarted();
}
