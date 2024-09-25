using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Mechanics.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.EntitySystem.Properties.Getters.Damage;

[Serializable]
[TypeId("e4eb8339ae4b4ac1be10c0efb45500eb")]
public abstract class CheckDamageGetter : PropertyGetter
{
	[EnumFlagsAsButtons(ColumnCount = 4)]
	public DamageTypeMask Types;

	[EnumFlagsAsButtons(ColumnCount = 4)]
	public DamageCategoryMask Categories;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check Damage Properties";
	}

	protected sealed override int GetBaseValue()
	{
		if (!Check(out var type, out var _, out var _))
		{
			return 0;
		}
		if (Types != 0 && (Types & type.GetInfo().Mask) == 0)
		{
			return 0;
		}
		if (Categories != 0 && (Categories & type.GetInfo().CategoryMask) == 0)
		{
			return 0;
		}
		return 1;
	}

	protected abstract bool Check(out DamageType type, [CanBeNull] out DamageData data, [CanBeNull] out RulebookEvent rule);
}
