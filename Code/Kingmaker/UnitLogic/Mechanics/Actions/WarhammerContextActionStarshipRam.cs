using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("1dff6f53c4285d14eaf190d693b96a5a")]
public class WarhammerContextActionStarshipRam : ContextAction
{
	public int Damage;

	public float BonusDamagePerCoveredCell = 1f;

	public override string GetCaption()
	{
		return "Starship Ram";
	}

	public override void RunAction()
	{
		StarshipEntity starshipEntity = (StarshipEntity)base.Context.MaybeCaster;
		if (starshipEntity == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return;
		}
		StarshipEntity starshipEntity2 = (StarshipEntity)base.Target.Entity;
		if (starshipEntity2 == null)
		{
			PFLog.Default.Error(this, "Target is missing");
			return;
		}
		int num = Damage;
		StarshipHitLocation hitLocation = StarshipHitLocation.Fore;
		if (starshipEntity2 != starshipEntity)
		{
			num += (int)(starshipEntity.CombatState.ActionPointsBlueSpentThisTurn * BonusDamagePerCoveredCell);
			hitLocation = Rulebook.Trigger(new RuleStarshipCalculateHitLocation(starshipEntity, starshipEntity2)).ResultHitLocation;
		}
		DealDamage(starshipEntity, starshipEntity2, num, hitLocation);
		starshipEntity.CombatState.SpendActionPointsAll(yellow: false, blue: true);
	}

	private void DealDamage(StarshipEntity initiator, StarshipEntity target, int damage, StarshipHitLocation hitLocation)
	{
		RuleStarshipCalculateDamageForTarget ruleStarshipCalculateDamageForTarget = Rulebook.Trigger(new RuleStarshipCalculateDamageForTarget(initiator, target, damage, DamageType.Ram, isAEAttack: false, hitLocation));
		Rulebook.Trigger(new RuleDealDamage(initiator, target, ruleStarshipCalculateDamageForTarget.ResultDamage));
	}
}
