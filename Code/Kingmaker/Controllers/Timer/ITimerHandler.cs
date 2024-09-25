using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.Timer;

public interface ITimerHandler : ISubscriber
{
	void SubscribeTimer(ITimer timer);
}
