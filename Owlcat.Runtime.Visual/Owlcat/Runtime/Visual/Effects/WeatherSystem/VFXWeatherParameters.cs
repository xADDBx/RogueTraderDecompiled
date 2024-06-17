using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public static class VFXWeatherParameters
{
	public static int BoundsSize = Shader.PropertyToID("System_BoundsSize");

	public static int ResetOwlcatFlagsCounter = Shader.PropertyToID("System_ResetOwlcatFlagsCounter");

	public static int Intensity = Shader.PropertyToID("System_Intensity");

	public static int WindIntensity = Shader.PropertyToID("System_WindIntensity");

	public static int WindDirection = Shader.PropertyToID("System_WindDirection");

	public static int LocationDataBoundsCenter = Shader.PropertyToID("System_LocationDataBoundsCenter");

	public static int LocationDataBoundsSize = Shader.PropertyToID("System_LocationDataBoundsSize");

	public static int LocationDataTexture = Shader.PropertyToID("System_LocationDataTexture");

	public static int LocationDataTextureSize = Shader.PropertyToID("System_LocationDataTextureSize");
}
