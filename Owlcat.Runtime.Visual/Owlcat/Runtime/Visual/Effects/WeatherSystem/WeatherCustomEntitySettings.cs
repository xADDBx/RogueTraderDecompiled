using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public abstract class WeatherCustomEntitySettings : ScriptableObject
{
	public abstract IWeatherEntityController GetController(Transform root);
}
