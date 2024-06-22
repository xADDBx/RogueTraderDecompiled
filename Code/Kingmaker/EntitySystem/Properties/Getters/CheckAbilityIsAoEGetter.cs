using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("a88aca1f99c04b5abe76a4f8ff634e37")]
public class CheckAbilityIsAoEGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool IncludeAreaEffects;

	public bool DoNotIncludeScatter;

	protected override int GetBaseValue()
	{
		AbilityData ability = this.GetAbility();
		if (ability == null)
		{
			return 0;
		}
		if ((ability.Blueprint.PatternSettings == null || (DoNotIncludeScatter && ability.Blueprint.PatternSettings is ScatterPattern)) && (!IncludeAreaEffects || !ability.IsAOE))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if ability has pattern";
	}
}
