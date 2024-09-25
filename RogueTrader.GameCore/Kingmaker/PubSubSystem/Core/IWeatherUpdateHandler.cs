using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IWeatherUpdateHandler : ISubscriber
{
	void OnUpdateWeatherSystem(bool overrideWeather);
}
