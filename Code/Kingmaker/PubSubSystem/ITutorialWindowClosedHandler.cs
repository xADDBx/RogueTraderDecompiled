using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;

namespace Kingmaker.PubSubSystem;

public interface ITutorialWindowClosedHandler : ISubscriber
{
	void HandleHideTutorial(TutorialData data);
}
