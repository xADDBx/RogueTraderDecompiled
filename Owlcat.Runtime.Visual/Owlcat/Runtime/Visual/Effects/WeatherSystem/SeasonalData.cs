using System;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class SeasonalData
{
	public WeatherArray InclemencyChangeSpeed = new WeatherArray
	{
		Values = new float[5] { 2f, 2f, 2f, 2f, 2f }
	};

	public WeatherArray InclemencyWeights = new WeatherArray
	{
		Values = new float[5] { 50f, 20f, 15f, 10f, 5f }
	};

	public WeatherMinMaxArray InclemencyCooldownInMinutes;
}
