using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;

namespace Kingmaker.PubSubSystem;

public interface INewTutorialUIHandler : ISubscriber
{
	void ShowTutorial(TutorialData data);

	void HideTutorial(TutorialData data);
}
