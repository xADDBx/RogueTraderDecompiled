using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("9c0a9fb4ae686c54c8e60a05c89de7c1")]
public class AbilityDeliverStarshipShot : AbilityDeliverEffect
{
	private class BurstTracker
	{
		public RuleStarshipPerformAttack firstAttack;

		public RuleStarshipPerformAttack lastAttack;

		public Projectile projectile;

		public RuleStarshipCalculateHitLocation hitLocationRule;

		public int hitsCount;
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.Caster is StarshipEntity starshipEntity))
		{
			PFLog.Default.ErrorWithReport("Caster starship is missing");
			yield break;
		}
		Vector3 castPosition = context.Caster.EyePosition;
		ItemEntityStarshipWeapon weapon = context.Ability.StarshipWeapon;
		if (weapon == null)
		{
			yield break;
		}
		List<StarshipFxLocator> list = context.Caster.View.gameObject.GetComponentsInChildren<StarshipFxLocator>().ToList();
		WeaponSlot weaponSlot = (weapon.HoldingSlot as WeaponSlot) ?? starshipEntity.Hull.WeaponSlots.Where((WeaponSlot slot) => slot.Type == WeaponSlotType.Dorsal).FirstOrDefault() ?? starshipEntity.Hull.WeaponSlots.FirstOrDefault();
		List<StarshipFxLocator> list2 = list.FindAll((StarshipFxLocator x) => x.weaponSlotType == weaponSlot.Type && x.starshipWeaponType == weapon.Blueprint.WeaponType);
		List<GameObject> finalShuffledLocators = new List<GameObject>();
		int num = weapon.Blueprint.DamageInstances * starshipEntity.TeamUnitsAlive + (weapon.IsFocusedEnergyWeapon ? 1 : 0);
		IEnumerable<StarshipBurstAttackCountIncrease> source = from bonus in starshipEntity.Facts.GetComponents<StarshipBurstAttackCountIncrease>()
			where bonus.weaponType == weapon.Blueprint.WeaponType
			where PFStatefulRandom.SpaceCombat.Range(0, 100) < bonus.Chances
			select bonus;
		num += source.Count();
		BurstTracker burstTracker = new BurstTracker
		{
			hitsCount = num
		};
		List<IEnumerator<AbilityDeliveryTarget>> deliveryProcesses;
		if (list2.Count > 0)
		{
			List<GameObject> list3 = new List<GameObject>();
			foreach (StarshipFxLocator item in list2)
			{
				if (!list3.Contains(item.particleMap.gameObject))
				{
					list3.Add(item.particleMap.gameObject);
				}
			}
			list3.Shuffle(PFStatefulRandom.UnitLogic.Abilities);
			int num2 = 0;
			while (num2 < num)
			{
				foreach (GameObject item2 in list3)
				{
					List<StarshipFxLocator> list4 = item2.GetComponentsInChildren<StarshipFxLocator>().ToList();
					list4.Shuffle(PFStatefulRandom.UnitLogic.Abilities);
					finalShuffledLocators.Add(list4[0].gameObject);
					num2++;
				}
				list3.Shuffle(PFStatefulRandom.UnitLogic.Abilities);
			}
			deliveryProcesses = Enumerable.Range(0, num).Select((int p, int i) => (!weapon.IsFocusedEnergyWeapon) ? DeliverDefault(context, finalShuffledLocators[i].transform.position, target, weapon.Ammo.Blueprint.ShotProjectile, i, burstTracker, useLaunchPosition: true) : DeliverLance(context, finalShuffledLocators[i].transform.position, target, weapon.Ammo.Blueprint.ShotProjectile, i, burstTracker, useLaunchPosition: true)).ToList();
		}
		else
		{
			deliveryProcesses = Enumerable.Range(0, num).Select((int p, int i) => (!weapon.IsFocusedEnergyWeapon) ? DeliverDefault(context, castPosition, target, weapon.Ammo.Blueprint.ShotProjectile, i, burstTracker) : DeliverLance(context, castPosition, target, weapon.Ammo.Blueprint.ShotProjectile, i, burstTracker)).ToList();
		}
		while (deliveryProcesses.Count > 0)
		{
			int i = 0;
			while (i < deliveryProcesses.Count)
			{
				IEnumerator<AbilityDeliveryTarget> p = deliveryProcesses[i];
				bool flag;
				while ((flag = p.MoveNext()) && p.Current != null)
				{
					yield return p.Current;
				}
				if (!flag)
				{
					deliveryProcesses[i] = null;
				}
				int num3 = i + 1;
				i = num3;
			}
			deliveryProcesses.RemoveAll((IEnumerator<AbilityDeliveryTarget> i) => i == null);
			yield return null;
		}
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverDefault(AbilityExecutionContext context, Vector3 customLaunchPosition, TargetWrapper targetWrapper, BlueprintProjectile projectileBlueprint, int index, BurstTracker burstTracker, bool useLaunchPosition = false)
	{
		TimeSpan startTime = Game.Instance.TimeController.GameTime;
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (!(maybeCaster is StarshipEntity caster))
		{
			PFLog.Default.Error(this, "Caster is missing");
			yield break;
		}
		maybeCaster = targetWrapper.Entity;
		if (!(maybeCaster is StarshipEntity target))
		{
			PFLog.Default.Error(this, "Target is missing");
			yield break;
		}
		ItemEntityStarshipWeapon weapon = context.Ability.StarshipWeapon;
		if (weapon == null)
		{
			PFLog.Default.Error(this, "Weapon is missing");
			yield break;
		}
		int num = index / weapon.Blueprint.ShotsInSeries;
		int num2 = index % weapon.Blueprint.ShotsInSeries;
		float delay = (float)num * weapon.Blueprint.DelayBetweenProjectiles + (float)num2 * weapon.Blueprint.DelayInSeries;
		while (Game.Instance.TimeController.GameTime - startTime < delay.Seconds())
		{
			yield return null;
		}
		MechanicEntity caster2 = context.Caster;
		RuleStarshipCalculateHitLocation ruleStarshipCalculateHitLocation = Rulebook.Trigger(new RuleStarshipCalculateHitLocation(caster, target));
		RuleStarshipPerformAttack starshipAttackRule = new RuleStarshipPerformAttack(caster, target, context.Ability, weapon, ruleStarshipCalculateHitLocation)
		{
			Reason = context
		};
		if (burstTracker.firstAttack == null)
		{
			burstTracker.firstAttack = starshipAttackRule;
		}
		starshipAttackRule.FirstAttackInBurst = burstTracker.firstAttack;
		starshipAttackRule.NextAttackInBurst = ((index == burstTracker.hitsCount - 1) ? null : starshipAttackRule);
		if (burstTracker.lastAttack != null)
		{
			burstTracker.lastAttack.NextAttackInBurst = starshipAttackRule;
		}
		burstTracker.lastAttack = starshipAttackRule;
		Projectile projectile = new ProjectileLauncher(projectileBlueprint, caster2, targetWrapper).Ability(context.Ability).LaunchPosition(useLaunchPosition ? new Vector3?(customLaunchPosition) : null).AttackResult(AttackResult.Hit)
			.MaxRangeCells(context.Ability.RangeCells)
			.Index(index)
			.StarshipHitLocation(ruleStarshipCalculateHitLocation.ResultHitLocation)
			.Launch();
		RulebookEventContext savedContext = Rulebook.CurrentContext.Clone();
		float distance = projectile.Distance(caster2.Position, targetWrapper.Point);
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
		{
			yield return null;
		}
		if (savedContext != null)
		{
			savedContext.Trigger(starshipAttackRule);
		}
		else
		{
			Rulebook.Trigger(starshipAttackRule);
		}
		yield return new AbilityDeliveryTarget(projectile.Target)
		{
			Projectile = projectile
		};
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverLance(AbilityExecutionContext context, Vector3 customLaunchPosition, TargetWrapper targetWrapper, BlueprintProjectile projectileBlueprint, int index, BurstTracker burstTracker, bool useLaunchPosition = false)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (!(maybeCaster is StarshipEntity caster))
		{
			PFLog.Default.Error(this, "Caster is missing");
			yield break;
		}
		maybeCaster = targetWrapper.Entity;
		if (!(maybeCaster is StarshipEntity target))
		{
			PFLog.Default.Error(this, "Target is missing");
			yield break;
		}
		ItemEntityStarshipWeapon weapon = context.Ability.StarshipWeapon;
		if (weapon == null)
		{
			PFLog.Default.Error(this, "Weapon is missing");
			yield break;
		}
		if (index == 0)
		{
			burstTracker.hitLocationRule = Rulebook.Trigger(new RuleStarshipCalculateHitLocation(caster, target));
			burstTracker.projectile = new ProjectileLauncher(projectileBlueprint, caster, targetWrapper).Ability(context.Ability).LaunchPosition(useLaunchPosition ? new Vector3?(customLaunchPosition) : null).AttackResult(AttackResult.Hit)
				.MaxRangeCells(context.Ability.RangeCells)
				.Index(index)
				.StarshipHitLocation(burstTracker.hitLocationRule.ResultHitLocation)
				.Launch();
		}
		TimeSpan startTime = Game.Instance.TimeController.GameTime;
		while (Game.Instance.TimeController.GameTime - startTime < (weapon.Blueprint.DelayBetweenProjectiles * (float)index).Seconds())
		{
			yield return null;
		}
		RuleStarshipPerformAttack ruleStarshipPerformAttack = new RuleStarshipPerformAttack(caster, target, context.Ability, weapon, burstTracker.hitLocationRule)
		{
			Reason = context
		};
		if (burstTracker.firstAttack == null)
		{
			burstTracker.firstAttack = ruleStarshipPerformAttack;
		}
		ruleStarshipPerformAttack.FirstAttackInBurst = burstTracker.firstAttack;
		ruleStarshipPerformAttack.NextAttackInBurst = ((index == burstTracker.hitsCount - 1) ? null : ruleStarshipPerformAttack);
		if (burstTracker.lastAttack != null)
		{
			burstTracker.lastAttack.NextAttackInBurst = ruleStarshipPerformAttack;
		}
		burstTracker.lastAttack = ruleStarshipPerformAttack;
		RulebookEventContext rulebookEventContext = Rulebook.CurrentContext.Clone();
		if (rulebookEventContext != null)
		{
			rulebookEventContext.Trigger(ruleStarshipPerformAttack);
		}
		else
		{
			Rulebook.Trigger(ruleStarshipPerformAttack);
		}
		yield return new AbilityDeliveryTarget(burstTracker.projectile.Target)
		{
			Projectile = burstTracker.projectile
		};
	}
}
