using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("e53aa7e81b714c5c9b883c8c3a03f1a4")]
public class HasBuffFromMyPetGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_Fact;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is UnitEntity unitEntity))
		{
			throw new Exception($"HasBuffFromMyPetGetter: can't find suitable target of type {Target}");
		}
		BaseUnitEntity pet = this.GetTargetByType(PropertyTargetType.ContextCaster)?.GetOptional<UnitPartPetOwner>()?.PetUnit;
		if (pet == null)
		{
			PFLog.Actions.Error("HasBuffFromMyPetGetter: can't find pet of context Caster");
			return 0;
		}
		unitEntity.Buffs.Enumerable.Any((Buff buffCheck) => buffCheck.Context.MaybeCaster == pet && buffCheck.Blueprint == m_Fact.GetBlueprint());
		if (!unitEntity.Buffs.Enumerable.Any((Buff buffCheck) => buffCheck.Context.MaybeCaster == pet && buffCheck.Blueprint == m_Fact.GetBlueprint()))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " has " + m_Fact.NameSafe() + " from pet of context caster";
	}
}
