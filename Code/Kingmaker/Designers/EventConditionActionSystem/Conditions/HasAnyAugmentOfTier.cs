using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("e404ee07ed1753f448d4cab7468b1011")]
public class HasAnyAugmentOfTier : Condition
{
	[SerializeField]
	private AugmentTier m_Tier;

	protected override string GetConditionCaption()
	{
		return $"Player has any augment of {m_Tier} in inventory";
	}

	protected override bool CheckCondition()
	{
		foreach (ItemEntity item in Game.Instance.Player.Inventory.Items)
		{
			if (item.Blueprint is BlueprintItemAugment blueprint && blueprint.GetComponent<EquipmentRestrictionAugmentTier>()?.AugmentTier >= m_Tier)
			{
				return true;
			}
		}
		return false;
	}
}
