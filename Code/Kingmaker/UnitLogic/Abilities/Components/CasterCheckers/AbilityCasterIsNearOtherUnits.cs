using System.Collections.Generic;
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
[AllowMultipleComponents]
[TypeId("98b5ee4683164fdb93c73d95591ad900")]
public class AbilityCasterIsNearOtherUnits : BlueprintComponent, IAbilityCasterRestriction
{
	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		List<BaseUnitEntity> source = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => AbilityCustomBladeDance.CheckEntityTargetable(caster, p)).ToList();
		List<MechanicEntity> source2 = Game.Instance.State.MechanicEntities.Where((MechanicEntity p) => AbilityCustomBladeDance.CheckEntityTargetable(caster, p)).ToList();
		if (!source.Any())
		{
			return source2.Any();
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.TargetsAroundRequired;
	}
}
