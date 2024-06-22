using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("455c9d4ea5184902aae705144c203b68")]
public class CheckAbilityCasterHasFactFromCasterOfThis : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalFact
{
	public BlueprintUnitFactReference FactToCheck;

	protected override int GetBaseValue()
	{
		IEnumerable<EntityFact> enumerable = this.GetAbility()?.Caster?.Facts.List.Where((EntityFact p) => p.Blueprint == FactToCheck.GetBlueprint());
		MechanicEntity mechanicEntity = this.GetFact()?.MaybeContext?.MaybeCaster;
		if (mechanicEntity == null || enumerable == null || enumerable.Empty())
		{
			return 0;
		}
		foreach (EntityFact item in enumerable)
		{
			if (item.MaybeContext?.MaybeCaster == mechanicEntity)
			{
				return 1;
			}
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability caster has fact from caster of the fact with the component";
	}
}
