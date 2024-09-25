using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Sound Effect")]
public class WeatherSoundSettings : WeatherCustomEntitySettings
{
	[SerializeField]
	private WeatherSoundType m_SoundType;

	public override IWeatherEntityController GetController(Transform root)
	{
		return new WeatherSoundController(root, m_SoundType);
	}
}
