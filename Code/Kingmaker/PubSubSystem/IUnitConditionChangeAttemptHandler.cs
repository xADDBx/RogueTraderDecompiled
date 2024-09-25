using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.PubSubSystem;

public interface IUnitConditionChangeAttemptHandler : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleUnitConditionAddAttempt(UnitCondition condition, bool success);
}
