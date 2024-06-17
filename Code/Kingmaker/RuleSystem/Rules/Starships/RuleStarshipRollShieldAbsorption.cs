using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipRollShieldAbsorption : RulebookTargetEvent<StarshipEntity, StarshipEntity>
{
	private const int MinChance = 30;

	private const int MaxChance = 100;

	public int Damage { get; }

	public DamageType DamageType { get; }

	public RuleRollD100 D100 { get; private set; }

	public bool Result { get; set; }

	public int ResultShields { get; private set; }

	public int ResultMaxShields { get; private set; }

	public int ResultChance { get; private set; }

	public int ResultAbsorbedOnFail { get; private set; }

	public int ResultAbsorbedDamage { get; private set; }

	public StarshipHitLocation ResultHitLocation { get; private set; }

	public bool IsPredictionOnly { get; set; }

	public bool IsFirstAttack { get; set; }

	public RuleStarshipRollShieldAbsorption([NotNull] StarshipEntity initiator, StarshipEntity target, int damage, DamageType damageType, StarshipHitLocation hitLocation = StarshipHitLocation.Undefined)
		: base(initiator, target)
	{
		Damage = damage;
		DamageType = damageType;
		ResultHitLocation = ((hitLocation != 0) ? hitLocation : Rulebook.Trigger(new RuleStarshipCalculateHitLocation(base.Initiator, base.Target)).ResultHitLocation);
	}

	public RuleStarshipRollShieldAbsorption Trigger(bool isPredictionOnly)
	{
		IsPredictionOnly = isPredictionOnly;
		Rulebook.Trigger(this);
		return this;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!base.Target.Shields.IsActive)
		{
			Result = false;
			return;
		}
		StarshipSectorShields starshipSectorShields = null;
		switch (ResultHitLocation)
		{
		case StarshipHitLocation.Fore:
			starshipSectorShields = base.Target.Shields.GetShields(StarshipSectorShieldsType.Fore);
			break;
		case StarshipHitLocation.Port:
			starshipSectorShields = base.Target.Shields.GetShields(StarshipSectorShieldsType.Port);
			break;
		case StarshipHitLocation.Starboard:
			starshipSectorShields = base.Target.Shields.GetShields(StarshipSectorShieldsType.Starboard);
			break;
		case StarshipHitLocation.Aft:
			starshipSectorShields = base.Target.Shields.GetShields(StarshipSectorShieldsType.Aft);
			break;
		}
		if (starshipSectorShields == null)
		{
			Result = false;
			return;
		}
		ResultMaxShields = starshipSectorShields.Max;
		ResultShields = starshipSectorShields.Current;
		if (ResultShields == 0)
		{
			Result = false;
			return;
		}
		if (DamageType == DamageType.Ram)
		{
			Result = true;
			ResultChance = 100;
			int resultAbsorbedOnFail = (ResultAbsorbedDamage = Math.Min(Damage, ResultShields / starshipSectorShields.RamAbsorbMod));
			ResultAbsorbedOnFail = resultAbsorbedOnFail;
			return;
		}
		ResultChance = 30 + 70 * ResultShields / ResultMaxShields;
		if (starshipSectorShields.Reinforced)
		{
			ResultChance += (100 - ResultChance) / 2;
		}
		if (!IsPredictionOnly)
		{
			D100 = Rulebook.Trigger(new RuleRollD100(base.Initiator));
			Result = D100.Result <= ResultChance;
		}
		else
		{
			Result = ResultChance > 0;
		}
		ResultAbsorbedDamage = ((ResultShields > ResultMaxShields / 2) ? Damage : (Damage / 2));
		ResultAbsorbedOnFail = ((ResultShields > ResultMaxShields / 2) ? (Damage / 2) : 0);
		ResultAbsorbedDamage = Math.Min(ResultAbsorbedDamage, ResultShields);
		ResultAbsorbedOnFail = Math.Min(ResultAbsorbedOnFail, ResultShields);
		if (ResultChance >= 100 && IsFirstAttack)
		{
			ResultAbsorbedOnFail = ResultAbsorbedDamage;
		}
		if (!Result)
		{
			ResultAbsorbedDamage = ResultAbsorbedOnFail;
		}
	}
}
