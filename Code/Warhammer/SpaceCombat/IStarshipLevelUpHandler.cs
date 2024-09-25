using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;

namespace Warhammer.SpaceCombat;

public interface IStarshipLevelUpHandler : ISubscriber<IStarshipEntity>, ISubscriber
{
	void HandleStarshipLevelUp(int newLevel, LevelUpManager manager);
}
