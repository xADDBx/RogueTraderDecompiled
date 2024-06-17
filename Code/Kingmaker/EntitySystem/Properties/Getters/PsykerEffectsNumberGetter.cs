using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("a66610907168b8245aa3c103a094b00f")]
public class PsykerEffectsNumberGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public bool OnlyFromCaster;

	[ShowIf("OnlyFromCaster")]
	public PropertyTargetType Caster;

	protected override int GetBaseValue()
	{
		IEnumerable<BlueprintAbility> source = new List<BlueprintAbility>();
		foreach (Buff buff in base.CurrentEntity.Buffs)
		{
			MechanicsContext maybeContext = buff.MaybeContext;
			if (maybeContext != null && maybeContext.SourceAbility != null && buff.MaybeContext.SourceAbility.AbilityParamsSource.HasFlag(WarhammerAbilityParamsSource.PsychicPower) && (buff.MaybeContext.MaybeCaster == this.GetTargetByType(Caster) || !OnlyFromCaster) && !source.Contains(buff.MaybeContext.SourceAbility))
			{
				source = source.Concat(buff.MaybeContext.SourceAbility);
			}
		}
		return source.Count();
	}

	protected override string GetInnerCaption()
	{
		return "Count number of buffs from Psyker powers";
	}
}
