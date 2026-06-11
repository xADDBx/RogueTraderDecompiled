using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.FlagCountable;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

public class AugmentSlot : EquipmentSlot<BlueprintItemAugment>, IHashable
{
	public readonly CountableFlag SpecialUnlock = new CountableFlag();

	private BlueprintAugmentSlot m_Blueprint;

	private bool m_WasAugmentedBefore;

	public BlueprintAugmentSlot Blueprint => m_Blueprint;

	public BlueprintItemAugment ItemBlueprint => base.MaybeItem?.Blueprint as BlueprintItemAugment;

	public bool IsDefaultAugmentEquipped => ItemBlueprint == DefaultAugment;

	private UnitAugments Augments => base.Owner.GetBodyOptional().Augments;

	private BlueprintItemAugment DefaultAugment => Blueprint?.DefaultAugment;

	public bool WasAugmentedBefore => m_WasAugmentedBefore;

	public AugmentSlot(BaseUnitEntity owner, BlueprintAugmentSlot blueprint)
		: base(owner)
	{
		m_Blueprint = blueprint;
	}

	public void InitializeOnPostLoad(BlueprintAugmentSlot blueprint)
	{
		m_Blueprint = blueprint;
		m_WasAugmentedBefore = HasItem;
	}

	public AugmentSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		if (base.IsItemSupported(item) && !Augments.Disabled)
		{
			return ((BlueprintItemAugment)item.Blueprint).AugmentSlot == Blueprint;
		}
		return false;
	}

	public override bool RemoveItem(bool autoMerge = true, bool force = false)
	{
		bool num = Blueprint.IsCommon() && !IsDefaultAugmentEquipped && m_WasAugmentedBefore;
		bool flag = (IsDefaultAugmentEquipped ? RemoveAndDestroyAugment(base.Item, autoMerge, force) : base.RemoveItem(autoMerge, force));
		if (num && flag)
		{
			ItemEntity item = DefaultAugment.CreateEntity();
			base.InsertItem(item);
		}
		return flag;
	}

	public void ApplyInsertion()
	{
		m_WasAugmentedBefore = true;
		EventBus.RaiseEvent(delegate(IAugmentEquipHandler handler)
		{
			handler.HandleAugmentEquip(ItemBlueprint);
		});
	}

	protected override void OnBeforeItemRemoved()
	{
		if (Augments.OverdriveSlot == m_Blueprint)
		{
			Augments.OverdriveSlot = null;
		}
	}

	private bool RemoveAndDestroyAugment(ItemEntity augment, bool autoMerge, bool force)
	{
		bool result = base.RemoveItem(autoMerge, force);
		Game.Instance.Player.Inventory.Remove(augment);
		return result;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
