using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;

namespace Kingmaker.PubSubSystem;

public interface ITutorialTriggerFailedHandler : ISubscriber
{
	void HandleLimitReached(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context);

	void HandleTagBanned(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context);

	void HandleFrequencyReached(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context);

	void HandleHigherPriorityCooldown(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context);

	void HandleLowerOrEqualPriorityCooldown(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context);
}
