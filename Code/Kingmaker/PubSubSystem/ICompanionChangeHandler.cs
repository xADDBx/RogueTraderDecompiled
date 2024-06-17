using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICompanionChangeHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleRecruit();

	void HandleUnrecruit();
}
public interface ICompanionChangeHandler<TTag> : ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<ICompanionChangeHandler, TTag>
{
}
