using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Interfaces;

public interface ISubscriber<TInvoker> : ISubscriber where TInvoker : IEntity
{
}
