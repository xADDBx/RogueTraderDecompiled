using JetBrains.Annotations;

namespace Kingmaker.PubSubSystem.Core;

public interface IRulebookTrigger
{
	TEvent Trigger<TEvent>([NotNull] RulebookEventContext context, [NotNull] TEvent evt) where TEvent : IRulebookEvent;
}
