using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.PubSubSystem;

public interface IUnitConditionsChanged : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleUnitConditionsChanged(UnitCondition condition);
}
public interface IUnitConditionsChanged<TTag> : IUnitConditionsChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitLifeStateChanged, TTag>
{
}
