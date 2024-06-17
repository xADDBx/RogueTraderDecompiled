using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IPartyHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAddCompanion();

	void HandleCompanionActivated();

	void HandleCompanionRemoved(bool stayInGame);

	void HandleCapitalModeChanged();
}
public interface IPartyHandler<TTag> : IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IPartyHandler, TTag>
{
}
