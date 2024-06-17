using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Wind Profile")]
public class WindProfile : ScriptableObject
{
	[SerializeField]
	private SeasonalData m_SeasonalData;

	public WeatherMinMaxArray WindIntensityRanges;

	public WeatherArray WindLerpValues;

	public NoiseSettings StrengthNoiseSettings;

	public NoiseSettings ShiftNoiseSettings;

	[Range(0f, 1f)]
	public float StrengthNoiseWeight = 0.5f;

	[Range(1f, 10f)]
	public float StrengthNoiseContrast = 1f;

	public SeasonalData SeasonalData => m_SeasonalData;
}
