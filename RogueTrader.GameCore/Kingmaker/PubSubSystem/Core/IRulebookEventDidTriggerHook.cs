using JetBrains.Annotations;

namespace Kingmaker.PubSubSystem.Core;

public interface IRulebookEventDidTriggerHook : IRulebookEventHook
{
	void OnAfterEventDidTrigger([NotNull] IRulebookEvent rule);
}
