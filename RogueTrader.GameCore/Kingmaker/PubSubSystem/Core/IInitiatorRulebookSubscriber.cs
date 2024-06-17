using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IInitiatorRulebookSubscriber
{
	[CanBeNull]
	IEntity GetSubscribingEntity();
}
