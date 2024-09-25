using System;
using JetBrains.Annotations;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipCalculateDamageForTarget : RulebookTargetEvent<StarshipEntity, StarshipEntity>
{
	public int OriginalMinDamage { get; }

	public int OriginalMaxDamage { get; }

	public int OriginalDeflection { get; }

	public DamageType DamageType { get; }

	public bool IsAEAttack { get; }

	public int BonusDamage { get; set; }

	public float ExtraDamageMod { get; set; }

	public DamageData ResultDamage { get; private set; }

	public int ResultMinDamage { get; private set; }

	public int ResultMaxDamage { get; private set; }

	public StarshipHitLocation ResultHitLocation { get; private set; }

	public int ResultDeflection { get; set; }

	public int DeflectionMultiplier { get; set; }

	public RuleStarshipCalculateDamageForTarget([NotNull] StarshipEntity initiator, [NotNull] StarshipEntity target, int minDamage, int maxDamage, DamageType damageType, bool isAEAttack = false, StarshipHitLocation hitLocation = StarshipHitLocation.Undefined)
		: base(initiator, target)
	{
		OriginalMinDamage = minDamage;
		OriginalMaxDamage = maxDamage;
		DamageType = damageType;
		IsAEAttack = isAEAttack;
		ResultHitLocation = ((hitLocation != 0) ? hitLocation : Rulebook.Trigger(new RuleStarshipCalculateHitLocation(base.Initiator, base.Target)).ResultHitLocation);
		ResultDamage = new DamageData(DamageType, 0);
		OriginalDeflection = base.Target.Hull.GetLocationDeflection(ResultHitLocation);
		ResultDeflection = CalculateResultDeflection(OriginalDeflection);
	}

	public RuleStarshipCalculateDamageForTarget([NotNull] StarshipEntity initiator, [NotNull] StarshipEntity target, int damage, DamageType damageType = DamageType.Direct, bool isAEAttack = false, StarshipHitLocation hitLocation = StarshipHitLocation.Undefined)
		: this(initiator, target, damage, damage, damageType, isAEAttack, hitLocation)
	{
	}

	public RuleStarshipCalculateDamageForTarget([NotNull] StarshipEntity initiator, [NotNull] StarshipEntity target, [NotNull] ItemEntityStarshipWeapon weapon)
		: this(initiator, target, weapon.Ammo?.Blueprint.Damage ?? 0, weapon.Ammo?.Blueprint.MaxDamage ?? 0, weapon.DamageType, weapon.IsAEAmmo)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (base.Target.Blueprint.IsSoftUnit && !IsAEAttack)
		{
			int resultMinDamage = (ResultMaxDamage = 1);
			ResultMinDamage = resultMinDamage;
		}
		else
		{
			int num2 = CalcDmg(OriginalMinDamage);
			int num3 = CalcDmg(OriginalMaxDamage);
			ResultMinDamage = Math.Max(0, num2 - ResultDeflection);
			ResultMaxDamage = Math.Max(ResultMinDamage, num3 - ResultDeflection);
		}
		ResultDamage = new DamageData(DamageType, ResultMinDamage, ResultMaxDamage);
		int CalcDmg(int srcDmg)
		{
			return (int)MathF.Round((float)(srcDmg + BonusDamage) * (1f + ExtraDamageMod));
		}
	}

	private int CalculateResultDeflection(int originalDeflection)
	{
		int num = DamageType switch
		{
			DamageType.Direct => 0, 
			DamageType.Warp => 0, 
			DamageType.Ram => originalDeflection * 2, 
			_ => originalDeflection, 
		};
		if (IsAEAttack)
		{
			num *= base.Target.TeamUnitsAlive;
		}
		return Mathf.RoundToInt((float)num * SpacecombatDifficultyHelper.StarshipDamageMod(base.Initiator));
	}
}
