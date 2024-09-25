using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("9dc80916f1707e44dad5e7780e90efaf")]
public class AbilityCustomStarshipShieldPulse : AbilityCustomLogic, IAbilityTargetRestriction
{
	[SerializeField]
	private float hitDelay;

	[SerializeField]
	private int ourShieldPctSpent;

	[SerializeField]
	private int aeldariHoloFieldPctRemoved;

	[SerializeField]
	private int aeldariMinimumRemoved;

	[SerializeField]
	private BlueprintBuffReference m_AeldariHoloFieldBuff;

	[SerializeField]
	private BlueprintFeatureReference m_DrukhariShadowFieldMark;

	[SerializeField]
	private BlueprintBuffReference m_NecronFallingApartBuff;

	[SerializeField]
	private int fallingApartShipHullDamagePct;

	[SerializeField]
	private ActionList ActionsOnSelf;

	public BlueprintBuff AeldariHoloFieldBuff => m_AeldariHoloFieldBuff?.Get();

	public BlueprintFeature DrukhariShadowFieldMark => m_DrukhariShadowFieldMark?.Get();

	public BlueprintBuff NecronFallingApartBuff => m_NecronFallingApartBuff?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private StarshipHitLocation GetCasterLoc(StarshipEntity caster, StarshipEntity target)
	{
		RuleStarshipCalculateHitLocation ruleStarshipCalculateHitLocation = new RuleStarshipCalculateHitLocation(target, caster);
		Rulebook.Trigger(ruleStarshipCalculateHitLocation);
		return ruleStarshipCalculateHitLocation.ResultHitLocation;
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.Caster is StarshipEntity starship))
		{
			yield break;
		}
		MechanicEntity entity = target.Entity;
		if (!(entity is StarshipEntity targetShip))
		{
			yield break;
		}
		double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)hitDelay)
		{
			yield return null;
		}
		StarshipHitLocation casterLoc = GetCasterLoc(starship, targetShip);
		StarshipSectorShields shields = starship.Shields.GetShields(casterLoc);
		shields.Damage = Math.Min(shields.Max, shields.Damage + shields.Max * ourShieldPctSpent / 100);
		PartStarshipShields shields2 = targetShip.Shields;
		if (shields2 != null && shields2.IsActive)
		{
			RuleStarshipCalculateHitLocation ruleStarshipCalculateHitLocation = new RuleStarshipCalculateHitLocation(starship, targetShip);
			Rulebook.Trigger(ruleStarshipCalculateHitLocation);
			StarshipSectorShields pushIt = shields2.GetShields(ruleStarshipCalculateHitLocation.ResultHitLocation);
			List<StarshipSectorShields> list = new List<StarshipSectorShields>
			{
				shields2.GetShields(StarshipSectorShieldsType.Fore),
				shields2.GetShields(StarshipSectorShieldsType.Aft),
				shields2.GetShields(StarshipSectorShieldsType.Port),
				shields2.GetShields(StarshipSectorShieldsType.Starboard)
			};
			list.RemoveAll((StarshipSectorShields s) => s == pushIt || s.Damage == 0);
			while (pushIt.Current > 0 && list.Count > 0)
			{
				if (pushIt.Current >= list.Count)
				{
					int num = list.Min((StarshipSectorShields s) => s.Damage);
					int putEach = Math.Min(num * list.Count, pushIt.Current) / list.Count;
					list.ForEach(delegate(StarshipSectorShields s)
					{
						s.Damage -= putEach;
					});
					pushIt.Damage += putEach * list.Count;
					list.RemoveAll((StarshipSectorShields s) => s.Damage == 0);
				}
				else
				{
					list.Take(pushIt.Current).ForEach(delegate(StarshipSectorShields s)
					{
						s.Damage--;
					});
					pushIt.Damage = pushIt.Max;
				}
			}
		}
		Buff buff = targetShip.Buffs.GetBuff(AeldariHoloFieldBuff);
		if (buff != null)
		{
			int num2 = Math.Max(buff.Rank * aeldariHoloFieldPctRemoved / 100, aeldariMinimumRemoved);
			if (buff.Rank > num2)
			{
				buff.RemoveRank(num2);
			}
			else
			{
				buff.Remove();
			}
		}
		if (targetShip.Facts.Get(DrukhariShadowFieldMark)?.FirstSource?.Fact is Buff buff2)
		{
			buff2.Remove();
		}
		if (targetShip.Buffs.GetBuff(NecronFallingApartBuff) != null)
		{
			int num3 = targetShip.Health.MaxHitPoints * fallingApartShipHullDamagePct / 100;
			Rulebook.Trigger(new RuleDealDamage(starship, targetShip, new DamageData(DamageType.Direct, num3, num3)));
		}
		using (context.GetDataScope(starship.ToITargetWrapper()))
		{
			ActionsOnSelf.Run();
		}
		yield return new AbilityDeliveryTarget(target);
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!(ability.Caster is StarshipEntity starshipEntity) || !(target.Entity is StarshipEntity target2) || !starshipEntity.Shields.IsActive)
		{
			return false;
		}
		StarshipHitLocation casterLoc = GetCasterLoc(starshipEntity, target2);
		StarshipSectorShields shields = starshipEntity.Shields.GetShields(casterLoc);
		return shields.Current * 100 / shields.Max >= ourShieldPctSpent;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.NotAllowedCellToCast;
	}
}
