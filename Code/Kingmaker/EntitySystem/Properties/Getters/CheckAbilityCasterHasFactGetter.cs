using System;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("8b735fb67eb84e3a908bab143b93ede0")]
public class CheckAbilityCasterHasFactGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public BlueprintFact CheckedFact;

	protected override int GetBaseValue()
	{
		if (!(this.GetAbility()?.Caster?.Facts.Contains(CheckedFact)).GetValueOrDefault())
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability caster has fact";
	}
}
