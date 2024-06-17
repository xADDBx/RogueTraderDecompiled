using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ITimeOfDayChangedHandler : ISubscriber
{
	void OnTimeOfDayChanged();
}
