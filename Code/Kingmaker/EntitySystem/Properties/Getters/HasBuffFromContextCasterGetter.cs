using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("e976e615d0274913a061b4841aa98d3e")]
public class HasBuffFromContextCasterGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_Fact;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is MechanicEntity mechanicEntity))
		{
			throw new Exception($"HasBuffFromContextCasterGetter: can't find suitable target of type {Target}");
		}
		Entity targetByType = this.GetTargetByType(PropertyTargetType.ContextCaster);
		MechanicEntity caster = targetByType as MechanicEntity;
		if (caster == null)
		{
			PFLog.Actions.Error("HasBuffFromContextCasterGetter: can't find context Caster");
			return 0;
		}
		mechanicEntity.Buffs.Enumerable.Any((Buff buffCheck) => buffCheck.Context.MaybeCaster == caster && buffCheck.Blueprint == m_Fact.GetBlueprint());
		if (!mechanicEntity.Buffs.Enumerable.Any((Buff buffCheck) => buffCheck.Context.MaybeCaster == caster && buffCheck.Blueprint == m_Fact.GetBlueprint()))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " has " + m_Fact.NameSafe() + " from context caster";
	}
}
