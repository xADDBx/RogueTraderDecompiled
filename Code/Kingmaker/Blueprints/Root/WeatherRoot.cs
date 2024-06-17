using System;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class WeatherRoot
{
	[Serializable]
	public class SeasonalData
	{
		public WeatherArray InclemencyWeights = new WeatherArray
		{
			Values = new float[5] { 50f, 20f, 15f, 10f, 5f }
		};

		public WeatherMinMaxArray InclemencyCooldownInMinutes;

		public int DayFrom;

		public int DayTo;
	}

	public WeatherArray InclemencyChangeSpeed = new WeatherArray
	{
		Values = new float[5] { 2f, 2f, 2f, 2f, 2f }
	};

	public SeasonalData Default;

	public SeasonalData Wind;

	[Header("Mechanics")]
	public InclemencyType ConcealmentBeginsOn = InclemencyType.Moderate;

	public InclemencyType StealthBonusBeginsOn = InclemencyType.Moderate;

	public int StealthCheckBonus = 6;

	public InclemencyType SlowdownBonusBeginsOn = InclemencyType.Storm;

	[SerializeField]
	[FormerlySerializedAs("RainPartyBuffs")]
	private BlueprintBuffReference[] m_RainPartyBuffs = new BlueprintBuffReference[5];

	[SerializeField]
	[FormerlySerializedAs("SnowPartyBuffs")]
	private BlueprintBuffReference[] m_SnowPartyBuffs = new BlueprintBuffReference[5];

	public ReferenceArrayProxy<BlueprintBuff> RainPartyBuffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] rainPartyBuffs = m_RainPartyBuffs;
			return rainPartyBuffs;
		}
	}

	public ReferenceArrayProxy<BlueprintBuff> SnowPartyBuffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] snowPartyBuffs = m_SnowPartyBuffs;
			return snowPartyBuffs;
		}
	}
}
