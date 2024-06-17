using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Obsolete;

namespace Kingmaker.PubSubSystem;

public interface IUnitLevelUpHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitBeforeLevelUp();

	void HandleUnitAfterLevelUp(LevelUpController controller);
}
