using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateScatterShotHitDirectionProbability : RulebookEvent
{
	public readonly ValueModifiersManager EffectiveBSModifiers = new ValueModifiersManager();

	public readonly AbilityData Ability;

	public readonly int ShotIndex;

	private bool m_IsResultOverridden;

	public int ResultMainLine { get; private set; }

	public int ResultScatterNear { get; private set; }

	public int ResultScatterFar { get; private set; }

	public int ResultMainLineNear => ResultMainLine + ResultScatterNear;

	private int EffectiveBS { get; set; }

	public RuleCalculateScatterShotHitDirectionProbability([NotNull] MechanicEntity initiator, AbilityData ability, int shotIndex)
		: base(initiator)
	{
		Ability = ability;
		ShotIndex = shotIndex;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!m_IsResultOverridden)
		{
			BlueprintCombatRoot combatRoot = BlueprintWarhammerRoot.Instance.CombatRoot;
			RuleCalculateStatsWeapon weaponStats = Ability.GetWeaponStats();
			int resultRecoil = weaponStats.ResultRecoil;
			RuleCalculateAttackPenalty ruleCalculateAttackPenalty = Rulebook.Trigger(new RuleCalculateAttackPenalty(base.ConcreteInitiator, Ability));
			int num = (base.ConcreteInitiator.GetAttributeOptional(StatType.WarhammerBallisticSkill)?.ModifiedValue * combatRoot.BallisticSkillPercentScaling / 100).GetValueOrDefault();
			if (ShotIndex > 0)
			{
				num -= resultRecoil;
			}
			num -= ruleCalculateAttackPenalty.ResultBallisticSkillPenalty;
			num += combatRoot.BallisticSkillBonus;
			num += EffectiveBSModifiers.Value;
			num += weaponStats.ResultAdditionalHitChance;
			EffectiveBS = Mathf.Clamp(num, combatRoot.MinEffectiveBallisticSkill, combatRoot.MaxEffectiveBallisticSkill);
			ResultMainLine = Math.Max(EffectiveBS / 2, combatRoot.MinResultMainLineChance);
			ResultScatterNear = EffectiveBS - ResultMainLine;
			ResultScatterFar = 100 - ResultMainLine - ResultScatterNear;
		}
	}

	public void OverrideResult(int main, int near, int far)
	{
		m_IsResultOverridden = true;
		if (main + near + far != 100)
		{
			PFLog.Default.ErrorWithReport("Invalid override values for scatter shot probabilities: main {0}, near {1}, far {2}", main, near, far);
		}
		ResultMainLine = main;
		ResultScatterNear = near;
		ResultScatterFar = far;
	}
}
