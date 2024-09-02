using Kingmaker.Blueprints.Root;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;

namespace Kingmaker.Controllers.FX;

public class SFXWrapperWeather : SFXWrapper
{
	private BlueprintWarpWeatherRoot m_WarpWeatherRoot;

	public override SoundPriority Priority => SoundPriority.Veil;

	public SFXWrapperWeather(BlueprintWarpWeatherRoot warpWeatherRoot)
	{
		m_WarpWeatherRoot = warpWeatherRoot;
	}

	public override void StartEvent()
	{
		SoundEventsManager.PostEvent(m_WarpWeatherRoot.WeatherSoundEventStart, VFXWeatherSystem.Instance.gameObject);
	}

	public override void StopEvent()
	{
		SoundEventsManager.PostEvent(m_WarpWeatherRoot.WeatherSoundEventStop, VFXWeatherSystem.Instance.gameObject);
	}

	public override string ToString()
	{
		return m_WarpWeatherRoot.WeatherSoundEventStart;
	}
}
