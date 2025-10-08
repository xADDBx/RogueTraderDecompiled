using System;
using Code.Enums;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.SystemMechanics;

[Serializable]
[TypeId("6aacab4e5a45411d94ccd135850df824")]
public class BlueprintCombatRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintCombatRoot>
	{
	}

	[Serializable]
	public class DOTEntry
	{
		public DOT Type;

		[SerializeField]
		[ValidateNotNull]
		private BlueprintBuffReference m_Buff;

		public BlueprintBuff Buff => m_Buff;
	}

	public int BaseOverpenetrationChance = 50;

	public int OverpenetrationReductionPerHit = 30;

	public int BurstNextBulletDodgePenalty = 20;

	public int BaseActionPointsRegen = 4;

	public int DistanceInPreparationTurn = 7;

	[Header("Initiative")]
	public InitiativeDistribution[] InitiativeDistributions = new InitiativeDistribution[0];

	[Header("Covers")]
	public int HitHalfCoverChance = 20;

	public int HitFullCoverChance = 50;

	[Header("Scatter Shot")]
	public int BallisticSkillBonus = 30;

	public int BallisticSkillPercentScaling = 100;

	public int MinEffectiveBallisticSkill = 30;

	public int MaxEffectiveBallisticSkill = 95;

	public int MinResultMainLineChance = 15;

	[Header("DOT Settings")]
	public DOTEntry[] DOTSettings = new DOTEntry[1]
	{
		new DOTEntry
		{
			Type = DOT.Bleeding
		}
	};

	[Header("Righteous Fury")]
	public int BaseRighteousFury = 10;

	public int HitChanceOverkillBorder = 95;

	[Header("Two Weapon Fighting")]
	public BlueprintAbilityGroupReference PrimaryHandAbilityGroup;

	public BlueprintAbilityGroupReference SecondaryHandAbilityGroup;

	public BlueprintAbilityGroupReference AdditionalLimbsAbilityGroup;

	[Header("Combat Log")]
	public BlueprintFeatureReference MedicineFeature;

	[Header("Misc")]
	public BlueprintAbilityGroupReference DamageOverTimeAbilityGroup;

	public BlueprintBuffReference AssassinKeystoneBuff;

	public BlueprintBuffReference AssassinKeystoneBuffOpening;
}
