using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Warhammer.SpaceCombat;

public interface IStarshipExpToNextLevelHandler : ISubscriber<IStarshipEntity>, ISubscriber
{
	void HandleStarshipExpToNextLevel(int currentLevel, int expToNextLevel, int gainedExp);
}
