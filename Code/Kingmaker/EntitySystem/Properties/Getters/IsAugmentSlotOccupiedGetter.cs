using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items.Slots;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1ed60d6df19e1b24d8185e56851d54af")]
public class IsAugmentSlotOccupiedGetter : MechanicEntityPropertyGetter
{
	[ValidateNotNull]
	[SerializeReference]
	private MechanicEntityEvaluator m_Unit;

	[SerializeField]
	private bool m_CheckAllSlots;

	[SerializeField]
	[Tooltip("If true, will instead check for empty slots")]
	private bool m_Reverse;

	[SerializeField]
	[HideIf("m_CheckAllSlots")]
	private BlueprintAugmentSlotReference[] m_Slots;

	public ReferenceArrayProxy<BlueprintAugmentSlot> Slots
	{
		get
		{
			BlueprintReference<BlueprintAugmentSlot>[] slots = m_Slots;
			return slots;
		}
	}

	protected override int GetBaseValue()
	{
		if (!(m_Unit?.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		int num = 0;
		if (m_CheckAllSlots)
		{
			num = baseUnitEntity.Body.Augments.Slots.Values.Sum((AugmentSlot slot) => SlotValue(slot.HasItem));
		}
		else
		{
			foreach (BlueprintAugmentSlot slot in Slots)
			{
				if (baseUnitEntity.Body.Augments.Slots.TryGetValue(slot, out var value))
				{
					num += SlotValue(value.HasItem);
				}
			}
		}
		return num;
		int SlotValue(bool hasItem)
		{
			if (!m_Reverse)
			{
				if (!hasItem)
				{
					return 0;
				}
				return 1;
			}
			if (!hasItem)
			{
				return 1;
			}
			return 0;
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = m_Unit?.GetCaption();
		string text2 = (m_Reverse ? "has no" : "has");
		string text3 = (m_CheckAllSlots ? "all slots" : string.Join(", ", Slots.Select((BlueprintAugmentSlot x) => x?.name)));
		return text + " " + text2 + " augments in " + text3 + ".";
	}
}
