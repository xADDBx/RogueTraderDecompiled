using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IInitiatorRulebookHandler<in T> : IRulebookHandler<T>, ISubscriber, IInitiatorRulebookSubscriber where T : IRulebookEvent
{
}
