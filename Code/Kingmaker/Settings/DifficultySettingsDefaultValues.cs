using System;
using Kingmaker.Enums;

namespace Kingmaker.Settings;

[Serializable]
public class DifficultySettingsDefaultValues
{
	public bool OnlyOneSave;

	public bool ImmersiveMode;

	public bool OnlyActiveCompanionsReceiveExperience;

	public bool OnlyInitiatorReceiveSkillCheckExperience;

	public bool LimitedAI;

	public int EnemyDodgePercentModifier;

	public int CoverHitBonusHalfModifier;

	public int CoverHitBonusFullModifier;

	public int MinPartyDamage;

	public int MinPartyDamageFraction;

	public int MinPartyStarshipDamage;

	public int MinPartyStarshipDamageFraction;

	public int PartyMomentumPercentModifier;

	public int NPCAttributesBaseValuePercentModifier;

	public HardCrowdControlDurationLimit HardCrowdControlOnPartyMaxDurationRounds;

	public int SkillCheckModifier;

	public int EnemyHitPointsPercentModifier;

	public int AllyResolveModifier;

	public int PartyDamageDealtAfterArmorReductionPercentModifier;

	public int WoundDamagePerTurnThresholdHPFraction;

	public int OldWoundDelayRounds;

	public int WoundStacksForTrauma;

	public int MinCR;

	public int MaxCR;

	public SpaceCombatDifficulty SpaceCombatDifficulty;

	public GameDifficultyOption GameDifficulty;
}
