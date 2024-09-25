using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ITargetRulebookHandler<in T> : IRulebookHandler<T>, ISubscriber, ITargetRulebookSubscriber where T : IRulebookEvent
{
}
