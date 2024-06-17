using JetBrains.Annotations;

namespace Kingmaker.PubSubSystem.Core;

public interface IRulebookEventAboutToTriggerHook : IRulebookEventHook
{
	void OnBeforeEventAboutToTrigger([NotNull] IRulebookEvent rule);
}
