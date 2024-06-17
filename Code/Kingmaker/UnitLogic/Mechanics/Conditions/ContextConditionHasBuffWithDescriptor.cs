using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("434d0c6f10abbd84b86cf64a99dc8833")]
public class ContextConditionHasBuffWithDescriptor : ContextCondition
{
	public SpellDescriptorWrapper SpellDescriptor;

	protected override string GetConditionCaption()
	{
		return string.Concat("Check if target has buffs with Descriptor");
	}

	protected override bool CheckCondition()
	{
		foreach (Buff buff in base.Target.Entity.Buffs)
		{
			if (buff.MaybeContext != null && (buff.MaybeContext.SpellDescriptor & SpellDescriptor) != Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells.SpellDescriptor.None)
			{
				return true;
			}
		}
		return false;
	}
}
