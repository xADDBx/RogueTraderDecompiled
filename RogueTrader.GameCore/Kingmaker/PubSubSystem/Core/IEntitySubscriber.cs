using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEntitySubscriber
{
	[CanBeNull]
	IEntity GetSubscribingEntity();
}
