using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateBurstCount : RulebookEvent
{
	public int BonusCount { get; set; }

	public int BaseBurstCount { get; }

	public AbilityData Ability { get; }

	public int Result { get; private set; }

	public RuleCalculateBurstCount([NotNull] MechanicEntity initiator, [NotNull] AbilityData ability)
		: base(initiator)
	{
		Ability = ability;
		BaseBurstCount = ability.RateOfFire;
	}

	public RuleCalculateBurstCount([NotNull] MechanicEntity initiator)
		: base(initiator)
	{
		BlueprintAbility blueprintAbility = (initiator.GetBodyOptional()?.PrimaryHand.MaybeWeapon)?.Blueprint.WeaponAbilities.FirstOrDefault()?.Ability;
		if (blueprintAbility != null)
		{
			Ability = initiator.Facts.Get<Ability>(blueprintAbility)?.Data;
			BaseBurstCount = Ability?.GetWeaponStats(initiator).ResultRateOfFire ?? 0;
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ItemEntityWeapon itemEntityWeapon = Ability?.Weapon;
		int val = ((itemEntityWeapon != null && itemEntityWeapon.Blueprint.WarhammerMaxAmmo > 1) ? Math.Max(itemEntityWeapon.CurrentAmmo, 2) : int.MaxValue);
		Result = Math.Min(BaseBurstCount + BonusCount, val);
	}
}
