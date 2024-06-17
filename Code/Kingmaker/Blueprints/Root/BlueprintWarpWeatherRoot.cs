using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("aa95453aa5bf40d3978f2617e1691963")]
public class BlueprintWarpWeatherRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintWarpWeatherRoot>
	{
	}

	[Serializable]
	public struct WeatherTypeByVailStruct
	{
		public InclemencyType WeatherType;

		public int Vail;
	}

	[Serializable]
	public struct WeatherEffectProfile
	{
		public VisualStateEffectType VisualStateEffectType;

		public WeatherProfile WeatherProfile;
	}

	[SerializeField]
	private WeatherProfileExtended m_WeatherProfile;

	[SerializeField]
	private WeatherTypeByVailStruct[] m_WeatherTypeByVail = new WeatherTypeByVailStruct[0];

	[SerializeField]
	public BlueprintBuffReference VeilTurnFlashingBuff;

	[AkEventReference]
	public string WeatherSoundEventStart;

	[AkEventReference]
	public string WeatherSoundEventStop;

	public float WeatherThreshold = 0.25f;

	public readonly int VeilBrokenThreshold = 15;

	[SerializeField]
	private WeatherEffectProfile[] m_WeatherEffectProfiles = new WeatherEffectProfile[0];

	public WeatherProfileExtended WeatherProfile => m_WeatherProfile;

	public WeatherTypeByVailStruct[] WeatherTypeByVail => m_WeatherTypeByVail;

	public InclemencyType GetInclemencyTypeByVail(int vail)
	{
		return WeatherTypeByVail.Last((WeatherTypeByVailStruct x) => x.Vail <= vail).WeatherType;
	}

	public bool TryGetWeatherEffectProfile(VisualStateEffectType effectType, out IWeatherProfile weatherProfile)
	{
		weatherProfile = m_WeatherEffectProfiles.FirstOrDefault((WeatherEffectProfile x) => x.VisualStateEffectType == effectType).WeatherProfile;
		return weatherProfile != null;
	}
}
