using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.UnitLogic.Parts;

public static class BonusAbilityExtension
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("BonusAbility");

	[CanBeNull]
	public static UnitPartBonusAbility GetBonusAbilityUseOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<UnitPartBonusAbility>();
	}

	public static bool HasAnyAvailableBonusAbility(this MechanicEntity currentUnit)
	{
		UnitPartBonusAbility bonusAbilityUseOptional = currentUnit.GetBonusAbilityUseOptional();
		if (bonusAbilityUseOptional == null)
		{
			return false;
		}
		foreach (Ability ability in bonusAbilityUseOptional.Owner.Abilities)
		{
			if (ability.Data.IsBonusUsage && ability.Data.IsAvailable)
			{
				return true;
			}
		}
		return false;
	}
}
