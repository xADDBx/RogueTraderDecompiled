using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Obsolete;

namespace Kingmaker.PubSubSystem;

public interface ILevelUpSelectClassHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleSelectClass(LevelUpState state);
}
