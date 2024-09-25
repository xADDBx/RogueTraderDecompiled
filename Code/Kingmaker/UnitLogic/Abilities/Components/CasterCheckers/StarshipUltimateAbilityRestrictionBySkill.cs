using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("8f6baaff377548b4aad95315c29b7aa4")]
public class StarshipUltimateAbilityRestrictionBySkill : BlueprintComponent, IAbilityCasterRestriction
{
	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starshipEntity))
		{
			return false;
		}
		return starshipEntity.Facts.GetComponents<StarshipCompanionsOnPostLogic>().FirstOrDefault()?.CanUseUltimate(starshipEntity, base.OwnerBlueprint as BlueprintAbility) ?? false;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.TooLowPostSkill;
	}
}
