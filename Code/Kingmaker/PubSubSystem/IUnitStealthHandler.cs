using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitStealthHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitSwitchStealthCondition(bool inStealth);
}
public interface IUnitStealthHandler<TTag> : IUnitStealthHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitStealthHandler, TTag>
{
}
