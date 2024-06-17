namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public interface IWeatherProfile
{
	SeasonalData SeasonalData { get; }

	WeatherMinMaxArray InclemencyIntensityRanges { get; }

	WeatherLayer[] Layers { get; }

	WeatherCustomEntitySettings[] CustomEffects { get; }

	WindProfile WindProfile { get; }

	WeatherDirectionalLightSettings DirectionalLightSettings { get; }

	VFXLocationWeatherDataProfile BakeProfile { get; }
}
