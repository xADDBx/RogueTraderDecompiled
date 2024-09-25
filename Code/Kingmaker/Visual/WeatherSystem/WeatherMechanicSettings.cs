using Kingmaker.Blueprints;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Mechanic Effect")]
public class WeatherMechanicSettings : WeatherCustomEntitySettings
{
	[Tooltip("The ability will trigger every this amount of seconds")]
	public double CastAbilityEvery = 60.0;

	[SerializeField]
	[FormerlySerializedAs("InclemencyType")]
	private InclemencyType m_InclemencyType = InclemencyType.Storm;

	[SerializeField]
	[FormerlySerializedAs("WeatherEffectsDifficulty")]
	private WeatherEffects m_WeatherEffectsDifficulty;

	[SerializeField]
	[FormerlySerializedAs("Target")]
	private WeatherAbilityTarget m_Target;

	[SerializeField]
	private BlueprintUnitReference m_BlueprintActor;

	[SerializeField]
	private BlueprintAbilityReference m_BlueprintAbility;

	[SerializeField]
	private BlueprintBuffReference m_BlueprintBuff;

	public InclemencyType InclemencyType => m_InclemencyType;

	public WeatherEffects WeatherEffectsDifficulty => m_WeatherEffectsDifficulty;

	public WeatherAbilityTarget Target => m_Target;

	public BlueprintUnit BlueprintActor => m_BlueprintActor;

	public BlueprintAbility BlueprintAbility => m_BlueprintAbility;

	public BlueprintBuff BlueprintBuff => m_BlueprintBuff;

	public override IWeatherEntityController GetController(Transform root)
	{
		return new WeatherMechanicController(this, root);
	}
}
