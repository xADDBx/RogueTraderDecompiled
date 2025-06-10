using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("c0eccc5499aa467e8cfe4ecd65c34147")]
public class HasBuffFromMyMasterGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_Fact;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is UnitEntity unitEntity))
		{
			throw new Exception($"HasBuffFromMyMasterGetter: can't find suitable target of type {Target}");
		}
		BaseUnitEntity master = (this.GetTargetByType(PropertyTargetType.ContextCaster) as BaseUnitEntity)?.Master;
		if (master == null)
		{
			PFLog.Actions.Error("HasBuffFromMyMasterGetter: can't find master of context Caster");
			return 0;
		}
		unitEntity.Buffs.Enumerable.Any((Buff buffCheck) => buffCheck.Context.MaybeCaster == master && buffCheck.Blueprint == m_Fact.GetBlueprint());
		if (!unitEntity.Buffs.Enumerable.Any((Buff buffCheck) => buffCheck.Context.MaybeCaster == master && buffCheck.Blueprint == m_Fact.GetBlueprint()))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " has " + m_Fact.NameSafe() + " from master of context caster";
	}
}
