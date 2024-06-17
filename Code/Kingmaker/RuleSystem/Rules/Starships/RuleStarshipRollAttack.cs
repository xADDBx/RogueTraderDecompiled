using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipRollAttack : RulebookTargetEvent<StarshipEntity, StarshipEntity>
{
	public ItemEntityStarshipWeapon Weapon { get; }

	public RuleStarshipCalculateHitChances HitChanceRule { get; private set; }

	public RuleRollD100 HitD100 { get; private set; }

	public RuleRollD100 CritD100 { get; private set; }

	public RuleRollD100 TargetDisruptionD100 { get; private set; }

	public int BonusTargetDisruptionChance { get; set; }

	public bool ResultIsHit { get; private set; }

	public bool ResultTargetDisruptionMiss { get; private set; }

	public bool ResultIsCrit { get; private set; }

	public bool IsPredictionOnly { get; set; }

	public bool IsTorpedoDirectHitAttempt { get; set; }

	public RuleStarshipRollAttack([NotNull] StarshipEntity initiator, [NotNull] StarshipEntity target, ItemEntityStarshipWeapon weapon)
		: base(initiator, target)
	{
		Weapon = weapon;
	}

	public void Trigger(bool isPredictionOnly, bool isTorpedoDirectHitAttempt)
	{
		IsPredictionOnly = isPredictionOnly;
		IsTorpedoDirectHitAttempt = isTorpedoDirectHitAttempt;
		Rulebook.Trigger(this);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		HitChanceRule = new RuleStarshipCalculateHitChances(base.Initiator, base.Target, Weapon)
		{
			IsTorpedoDirectHitAttempt = IsTorpedoDirectHitAttempt
		};
		Rulebook.Trigger(HitChanceRule);
		if (!IsPredictionOnly)
		{
			HitD100 = Rulebook.Trigger(new RuleRollD100(base.Initiator));
			CritD100 = Rulebook.Trigger(new RuleRollD100(base.Initiator));
			ResultIsHit = HitD100.Result <= HitChanceRule.ResultHitChance;
			ResultIsCrit = ResultIsHit && CritD100.Result <= HitChanceRule.ResultCritChance;
		}
		else
		{
			ResultIsHit = HitChanceRule.ResultHitChance > 0;
			ResultIsCrit = ResultIsHit && HitChanceRule.ResultCritChance >= 100;
		}
		if (ResultIsHit && BonusTargetDisruptionChance > 0 && !Weapon.IsAEAmmo && !IsPredictionOnly)
		{
			TargetDisruptionD100 = Rulebook.Trigger(new RuleRollD100(base.Initiator));
			if (TargetDisruptionD100.Result <= BonusTargetDisruptionChance)
			{
				ResultIsHit = false;
				ResultIsCrit = false;
				ResultTargetDisruptionMiss = true;
			}
		}
	}
}
