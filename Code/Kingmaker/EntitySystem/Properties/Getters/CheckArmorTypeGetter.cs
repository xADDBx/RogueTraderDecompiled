using System;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items.Slots;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("8703e4add55040949b18648d71bcdd3e")]
public class CheckArmorTypeGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	[SerializeField]
	private WarhammerArmorCategory m_ArmorType;

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Default.Error("CheckArmorTypeGetter: target unit is null (not a BaseUnitEntity)");
			return 0;
		}
		if (m_ArmorType == WarhammerArmorCategory.None)
		{
			ArmorSlot armorSlot = baseUnitEntity?.Body.Armor;
			if (armorSlot == null || !armorSlot.HasArmor)
			{
				return 1;
			}
		}
		if (baseUnitEntity.Body.Armor.MaybeArmor?.Blueprint.Category != m_ArmorType)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if ArmorType of " + Target.Colorized() + " is " + m_ArmorType;
	}
}
