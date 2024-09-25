using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IWeatherChangeHandler : ISubscriber
{
	void OnWeatherChange();
}
