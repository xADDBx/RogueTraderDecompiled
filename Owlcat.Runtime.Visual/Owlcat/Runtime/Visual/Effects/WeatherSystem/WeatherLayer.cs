using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Layer")]
public class WeatherLayer : ScriptableObject
{
	public FogSettings FogSettings;

	public DecalSettings DecalSettings;

	public PostProcessVolumeSettings PostProcessVolumeSettings;

	public List<WeatherEffect> Effects;

	public List<WeatherCustomEntitySettings> CustomEffects;
}
