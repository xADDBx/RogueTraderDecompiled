using System;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class WeatherArray
{
	public float[] Values;

	public float this[InclemencyType i] => Values[(int)i];
}
