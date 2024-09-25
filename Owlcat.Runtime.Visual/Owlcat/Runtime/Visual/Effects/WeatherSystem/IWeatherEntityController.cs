using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public interface IWeatherEntityController : IDisposable
{
	void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity);
}
