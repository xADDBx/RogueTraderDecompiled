using System;
using JetBrains.Annotations;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipCalculateHitChances : RulebookTargetEvent<StarshipEntity, StarshipEntity>
{
	public ItemEntityStarshipWeapon Weapon { get; }

	public int ResultHitChance { get; private set; }

	public int ResultCritChance { get; private set; }

	public int ResultEvasionChance { get; private set; }

	public int BonusHitChance { get; set; }

	public int BonusCritChance { get; set; }

	public float CritAdditionalMod { get; set; }

	public int BonusEvasionChance { get; set; }

	public bool SuperEvasion { get; set; }

	public bool IsTorpedoDirectHitAttempt { get; set; }

	public RuleStarshipCalculateHitChances([NotNull] StarshipEntity initiator, [NotNull] StarshipEntity target, ItemEntityStarshipWeapon weapon)
		: base(initiator, target)
	{
		Weapon = weapon;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ResultHitChance = CalculateInitiatorHitChances() + BonusHitChance;
		ResultCritChance = Mathf.RoundToInt((float)(CalculateInitiatorCritChances() + BonusCritChance) * (1f + CritAdditionalMod));
		ResultEvasionChance = CalculateTargetEvasion(Weapon) + BonusEvasionChance;
		ResultEvasionChance = Math.Clamp(ResultEvasionChance, 0, 100);
		ItemEntityStarshipWeapon weapon = Weapon;
		if (weapon != null && !weapon.IsAEAmmo)
		{
			if (IsTorpedoDirectHitAttempt && base.Target.IsSoftUnit)
			{
				ResultHitChance = 0;
				ResultCritChance = 0;
			}
			else
			{
				if (Weapon.Ammo.Blueprint.IsIgnoreEvasion)
				{
					ResultEvasionChance = (SuperEvasion ? (ResultEvasionChance * 60 / 100) : 0);
				}
				ResultHitChance = ResultHitChance * (100 - ResultEvasionChance) / 100;
			}
		}
		else
		{
			ResultHitChance = 100;
			ResultCritChance = ResultCritChance * (100 - ResultEvasionChance) / 100;
		}
		ResultHitChance = Math.Clamp(ResultHitChance, 0, 100);
		ResultCritChance = Math.Clamp(ResultCritChance, 0, 100);
	}

	public int CalculateInitiatorHitChances()
	{
		return base.Initiator.AugerArray?.hitChances ?? 50;
	}

	public int CalculateInitiatorCritChances()
	{
		return base.Initiator.AugerArray?.critChances ?? 0;
	}

	public int CalculateTargetEvasion(ItemEntityStarshipWeapon weapon)
	{
		return Mathf.RoundToInt((float)base.Target.Starship.Evasion * SpacecombatDifficultyHelper.StarshipAvoidanceMod(base.Target));
	}
}
