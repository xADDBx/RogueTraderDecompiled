using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IGlobalRulebookHandler<in T> : IRulebookHandler<T>, ISubscriber, IGlobalRulebookSubscriber where T : IRulebookEvent
{
}
