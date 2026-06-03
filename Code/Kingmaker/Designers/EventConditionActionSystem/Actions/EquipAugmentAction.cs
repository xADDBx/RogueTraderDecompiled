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

[TypeId("106e0f865d5fe0042bb5918e190f606e")]
public class EquipAugmentAction : GameAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private BlueprintItemAugmentReference[] m_Items;

	[SerializeField]
	[Tooltip("Если True - экипирует аугмент из инвентаря. Если False - создает аугмент из воздуха.")]
	private bool m_EquipFromInventory;

	[SerializeField]
	[Tooltip("Если True - аугмент будет надет только если слот свободен. Если False - заменит уже надетый аугмент.")]
	private bool m_RequiresEmptySlot;

	public override string GetCaption()
	{
		string text = string.Join(",", m_Items.Select(delegate(BlueprintItemAugmentReference item)
		{
			LocalizedString localizedString = item?.Get()?.LocalizedName;
			return (localizedString == null) ? "NONE" : ((string)localizedString);
		}));
		string caption = m_Unit.GetCaption();
		return "equip " + text + " on " + caption;
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
		foreach (BlueprintItemAugmentReference blueprintItemAugmentReference in items)
		{
			BlueprintItemAugment augment = blueprintItemAugmentReference.Get();
			BlueprintAugmentSlot augmentSlot = augment.AugmentSlot;
			if (m_EquipFromInventory && !Game.Instance.Player.Inventory.Contains(augment))
			{
				continue;
			}
			PartUnitBody bodyOptional = value.GetBodyOptional();
			if (bodyOptional != null && bodyOptional.Augments?.Slots.TryGetValue(augmentSlot, out value2) == true)
			{
				if (!m_RequiresEmptySlot || !value2.HasItem)
				{
					ItemEntity item = (m_EquipFromInventory ? Game.Instance.Player.Inventory.FirstOrDefault((ItemEntity x) => x.Blueprint == augment) : augment.CreateEntity());
					value2.InsertItem(item);
				}
			}
			else
			{
				PFLog.Items.Error($"Cannot equip augment {augment} on {value} because it doesn't have a slot {augmentSlot}");
			}
		}
	}
}
