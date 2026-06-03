using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("21e46c9228d1ed04ea8f0c6e5c2c5610")]
public class UnequipAugmentAction : GameAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private BlueprintItemAugmentReference[] m_Items;

	[SerializeField]
	private bool m_ReturnToInventory;

	public override string GetCaption()
	{
		string text = string.Join(",", m_Items.Select(delegate(BlueprintItemAugmentReference item)
		{
			LocalizedString localizedString = item?.Get()?.LocalizedName;
			return (localizedString == null) ? "NONE" : ((string)localizedString);
		}));
		string caption = m_Unit.GetCaption();
		return "unequip " + text + " from " + caption;
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		if (value == null)
		{
			return;
		}
		BlueprintItemAugmentReference[] items = m_Items;
		AugmentSlot value2 = default(AugmentSlot);
		for (int i = 0; i < items.Length; i++)
		{
			BlueprintItemAugment blueprintItemAugment = items[i].Get();
			BlueprintAugmentSlot augmentSlot = blueprintItemAugment.AugmentSlot;
			PartUnitBody bodyOptional = value.GetBodyOptional();
			if (bodyOptional != null && bodyOptional.Augments?.Slots.TryGetValue(augmentSlot, out value2) == true)
			{
				if (value2.Item != null && value2.Item.Blueprint == blueprintItemAugment)
				{
					value2.RemoveItem();
					if (!m_ReturnToInventory)
					{
						Game.Instance.Player.Inventory.Remove(blueprintItemAugment);
					}
				}
				else
				{
					PFLog.Items.Error($"Cannot unequip augment {blueprintItemAugment} from {value} because it is not equipped in slot {augmentSlot}");
				}
			}
			else
			{
				PFLog.Items.Error($"Cannot unequip augment {blueprintItemAugment} from {value} because it doesn't have a slot {augmentSlot}");
			}
		}
	}
}
