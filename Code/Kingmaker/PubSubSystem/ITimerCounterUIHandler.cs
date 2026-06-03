using Kingmaker.AreaLogic.Etudes;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ITimerCounterUIHandler : ISubscriber
{
	void ShowTimerCounter(TimerShowCounterUIStruct counterUIStruct);

	void HideTimerCounter(string id);
}
