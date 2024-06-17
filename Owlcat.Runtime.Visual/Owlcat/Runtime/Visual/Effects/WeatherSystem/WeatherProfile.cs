using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Weather Profile")]
public class WeatherProfile : ScriptableObject, IWeatherProfile
{
	[SerializeField]
	private SeasonalData m_SeasonalData;

	[SerializeField]
	private WeatherMinMaxArray m_InclemencyIntensityRanges;

	[SerializeField]
	private WeatherLayer[] m_Layers;

	[SerializeField]
	private WeatherCustomEntitySettings[] m_CustomEffects;

	[SerializeField]
	private WeatherDirectionalLightSettings m_DirectionalLightSettings;

	[SerializeField]
	private WindProfile m_WindProfile;

	[SerializeField]
	private VFXLocationWeatherDataProfile m_BakeProfile;

	public SeasonalData SeasonalData => m_SeasonalData;

	public WeatherLayer[] Layers => m_Layers;

	public WeatherCustomEntitySettings[] CustomEffects => m_CustomEffects;

	public WindProfile WindProfile => m_WindProfile;

	public WeatherMinMaxArray InclemencyIntensityRanges => m_InclemencyIntensityRanges;

	public WeatherDirectionalLightSettings DirectionalLightSettings => m_DirectionalLightSettings;

	public VFXLocationWeatherDataProfile BakeProfile => m_BakeProfile;

	public string LegacyName => string.Concat(base.name + "_" + GetInstanceID().ToString().Substring(0, 2));
}
