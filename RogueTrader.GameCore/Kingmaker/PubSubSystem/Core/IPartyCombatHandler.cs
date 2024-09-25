using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IPartyCombatHandler : ISubscriber
{
	void HandlePartyCombatStateChanged(bool inCombat);
}
public interface IPartyCombatHandler<TTag> : IPartyCombatHandler, ISubscriber, IEventTag<IPartyCombatHandler, TTag>
{
}
