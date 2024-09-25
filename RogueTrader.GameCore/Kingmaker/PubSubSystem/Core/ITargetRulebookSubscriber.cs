using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ITargetRulebookSubscriber
{
	[CanBeNull]
	IEntity GetSubscribingEntity();
}
