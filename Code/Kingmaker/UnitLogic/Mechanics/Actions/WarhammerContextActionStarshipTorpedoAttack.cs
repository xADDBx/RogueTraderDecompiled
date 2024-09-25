using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("a2e56ff6906d4314197e5410724905c6")]
public class WarhammerContextActionStarshipTorpedoAttack : ContextAction
{
	public override string GetCaption()
	{
		return "Perform an attack with a starship torpedo weapon that gave this ability";
	}

	protected override void RunAction()
	{
		StarshipEntity caster = base.Context.MaybeCaster as StarshipEntity;
		if (caster == null)
		{
			return;
		}
		StarshipEntity target = base.Target.Entity as StarshipEntity;
		if (target == null)
		{
			return;
		}
		AbilityData ability = base.Context.SourceAbilityContext.Ability;
		if ((object)ability == null)
		{
			return;
		}
		ItemEntityStarshipWeapon weapon = ability?.StarshipWeapon;
		if (weapon == null)
		{
			return;
		}
		ItemEntityStarshipAmmo fakeAmmo = weapon.FakeAmmo;
		ItemEntityStarshipAmmo itemEntityStarshipAmmo = (ItemEntityStarshipAmmo)(weapon.Blueprint.AlternateAmmo?.CreateEntity());
		RuleStarshipPerformAttack firstAttack = null;
		RuleStarshipPerformAttack lastAttack = null;
		List<RuleStarshipPerformAttack> attacks = new List<RuleStarshipPerformAttack>();
		for (int i = 0; i < caster.TeamUnitsAlive; i++)
		{
			RuleStarshipPerformAttack ruleStarshipPerformAttack = AddAttackRule();
			ruleStarshipPerformAttack.IsTorpedoDirectHitAttempt = true;
			ruleStarshipPerformAttack.AttackRollRule.Trigger(isPredictionOnly: false, isTorpedoDirectHitAttempt: true);
		}
		if (itemEntityStarshipAmmo != null)
		{
			for (int j = 0; j < caster.TeamUnitsAlive; j++)
			{
				if (!attacks[j].AttackRollRule.ResultIsHit)
				{
					AddAttackRule();
				}
			}
		}
		for (int k = 0; k < caster.TeamUnitsAlive; k++)
		{
			Rulebook.Trigger(attacks[k]);
		}
		weapon.FakeAmmo = itemEntityStarshipAmmo;
		for (int l = caster.TeamUnitsAlive; l < attacks.Count; l++)
		{
			Rulebook.Trigger(attacks[l]);
		}
		weapon.FakeAmmo = fakeAmmo;
		RuleStarshipPerformAttack AddAttackRule()
		{
			RuleStarshipPerformAttack ruleStarshipPerformAttack2 = new RuleStarshipPerformAttack(caster, target, ability, weapon);
			if (firstAttack == null)
			{
				firstAttack = ruleStarshipPerformAttack2;
			}
			ruleStarshipPerformAttack2.FirstAttackInBurst = firstAttack;
			if (lastAttack != null)
			{
				lastAttack.NextAttackInBurst = ruleStarshipPerformAttack2;
			}
			lastAttack = ruleStarshipPerformAttack2;
			attacks.Add(ruleStarshipPerformAttack2);
			return ruleStarshipPerformAttack2;
		}
	}
}
