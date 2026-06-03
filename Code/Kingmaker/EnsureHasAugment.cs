using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using UnityEngine;

namespace Kingmaker;

[TypeId("42eee5a99a6f6884cab4ba1006460027")]
public class EnsureHasAugment : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private BlueprintItemAugmentReference _augmentItem;

	public BlueprintItemAugment AugmentItem => _augmentItem.Get();

	public override string GetCaption()
	{
		return $"Ensure {m_Unit} has {_augmentItem}";
	}

	protected override void RunActionOverride()
	{
		PartUnitBody bodyOptional = m_Unit.GetValue().GetBodyOptional();
		if (bodyOptional == null)
		{
			PFLog.EntityFact.Error($"Augment insertion failed: failed to get unit's body optional: {m_Unit}");
			return;
		}
		BlueprintAugmentSlot augmentSlot = AugmentItem.AugmentSlot;
		AugmentSlot augmentSlot2 = bodyOptional.Augments.Slots[augmentSlot];
		if (augmentSlot2.MaybeItem != null)
		{
			PFLog.EntityFact.Error($"Augment slot {augmentSlot} is already occupied by " + $"{augmentSlot2.MaybeItem.Blueprint} for unit {m_Unit}! " + $"Cannot insert augment {AugmentItem}");
			return;
		}
		ItemEntity item = AugmentItem.CreateEntity();
		augmentSlot2.InsertItem(item, force: true);
	}
}
