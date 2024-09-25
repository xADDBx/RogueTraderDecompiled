using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.PubSubSystem;

public interface IUnitLifeStateChanged : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleUnitLifeStateChanged(UnitLifeState prevLifeState);
}
public interface IUnitLifeStateChanged<TTag> : IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitLifeStateChanged, TTag>
{
}
