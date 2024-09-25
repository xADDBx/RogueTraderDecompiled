using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UnitLogic.Parts;

public interface ICompanionStateChanged : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleCompanionStateChanged();
}
public interface ICompanionStateChanged<TTag> : ICompanionStateChanged, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<ICompanionStateChanged, TTag>
{
}
