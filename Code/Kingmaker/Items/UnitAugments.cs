using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

public class UnitAugments : IHashable
{
	[JsonProperty]
	private Dictionary<BlueprintAugmentSlot, AugmentSlot> m_Slots;

	[JsonProperty]
	private BlueprintAugmentSlot m_OverdriveSlotBp;

	[JsonProperty]
	private Ability m_OverdriveAbility;

	public readonly CountableFlag Disabled = new CountableFlag();

	public BaseUnitEntity Owner { get; private set; }

	public IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> Slots => m_Slots;

	public BlueprintAugmentSlot OverdriveSlot
	{
		get
		{
			return m_OverdriveSlotBp;
		}
		set
		{
			SetOverdriveSlot(value);
		}
	}

	public Ability OverdriveAbility => m_OverdriveAbility;

	[JsonConstructor]
	private UnitAugments()
	{
	}

	public UnitAugments(BaseUnitEntity owner)
	{
		Owner = owner;
		m_Slots = BlueprintRoot.Instance.Augments.CommonSlots.ToDictionary((BlueprintAugmentSlot slotType) => slotType, (BlueprintAugmentSlot slotType) => new AugmentSlot(Owner, slotType));
	}

	private void SetOverdriveSlot(BlueprintAugmentSlot slotBp)
	{
		if (slotBp == null)
		{
			DeactivateOverdriveAbility();
			return;
		}
		if (!m_Slots.TryGetValue(slotBp, out var value))
		{
			PFLog.Items.Error($"Couldn't find augment slot: {slotBp}");
			return;
		}
		if (!value.HasItem)
		{
			PFLog.Items.Error($"Cannot set slot {slotBp} as overdrive because it is empty!");
			return;
		}
		BlueprintAbility overdriveAbility = value.ItemBlueprint.OverdriveAbility;
		if (overdriveAbility == null)
		{
			PFLog.Items.Error($"Cannot set slot {slotBp} as overdrive because {value.ItemBlueprint} has no overdrive ability!");
		}
		else
		{
			ActivateOverdriveAbility(value.Item, overdriveAbility, slotBp);
		}
	}

	private void ActivateOverdriveAbility(ItemEntity item, BlueprintAbility abilityBp, BlueprintAugmentSlot slotBp)
	{
		DeactivateOverdriveAbility();
		Ability abilityFact = Owner.Facts.Add(new Ability(abilityBp, Owner));
		if (abilityFact != null)
		{
			abilityFact.AddSource(item);
			item.Abilities.RemoveAll((Ability o) => o.Blueprint == abilityFact.Blueprint);
			item.Abilities.Add(abilityFact);
			m_OverdriveAbility = abilityFact;
			m_OverdriveSlotBp = slotBp;
			EventBus.RaiseEvent(delegate(IUnitOverdriveAugmentHandler h)
			{
				h.HandleAugmentActivateOverdrive(Owner);
			});
		}
	}

	private void DeactivateOverdriveAbility()
	{
		if (m_OverdriveSlotBp != null && m_OverdriveAbility != null)
		{
			if (!m_Slots.TryGetValue(m_OverdriveSlotBp, out var value))
			{
				throw new Exception("Overdrive slot doesn't exist");
			}
			value.Item.Abilities.Remove(m_OverdriveAbility);
			Owner.Facts.Remove(m_OverdriveAbility);
			m_OverdriveAbility = null;
			m_OverdriveSlotBp = null;
			EventBus.RaiseEvent(delegate(IUnitOverdriveAugmentHandler h)
			{
				h.HandleAugmentDeactivateOverdrive(Owner);
			});
		}
	}

	public void RetainSpecialSlot(BlueprintAugmentSlot slotType)
	{
		if (ValidateSpecialSlot(slotType))
		{
			GetOrCreateSlot(slotType).SpecialUnlock.Retain();
		}
	}

	public AugmentSlot GetOrCreateSlot(BlueprintAugmentSlot slotType)
	{
		if (m_Slots.TryGetValue(slotType, out var value))
		{
			return value;
		}
		m_Slots.Add(slotType, value = new AugmentSlot(Owner, slotType));
		Owner.Body.AllSlots.Add(value);
		return value;
	}

	public void ReleaseSpecialSlot(BlueprintAugmentSlot slotType)
	{
		if (ValidateSpecialSlot(slotType) && m_Slots.TryGetValue(slotType, out var value))
		{
			value.SpecialUnlock.Release();
			RemoveSpecialSlotIfReleased(slotType, value);
		}
	}

	private void RemoveSpecialSlotIfReleased(BlueprintAugmentSlot slotType, AugmentSlot slot)
	{
		if (!slot.SpecialUnlock.Value)
		{
			if (slot.HasItem)
			{
				slot.RemoveItem(autoMerge: true, force: true);
			}
			Owner.Body.AllSlots.Remove(slot);
			m_Slots.Remove(slotType);
		}
	}

	private static bool ValidateSpecialSlot(BlueprintAugmentSlot slotBp)
	{
		if (slotBp.IsCommon())
		{
			PFLog.Items.Error($"Trying to use common {slotBp} as a special slot.");
			return false;
		}
		return true;
	}

	public void OnPrePostLoad(BaseUnitEntity owner)
	{
		Owner = owner;
	}

	public void OnPostLoad()
	{
		foreach (KeyValuePair<BlueprintAugmentSlot, AugmentSlot> slot in m_Slots)
		{
			if (!slot.Key.IsCommon() && !slot.Value.SpecialUnlock.Value)
			{
				RemoveSpecialSlotIfReleased(slot.Key, slot.Value);
			}
			else
			{
				slot.Value.InitializeOnPostLoad(slot.Key);
			}
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Dictionary<BlueprintAugmentSlot, AugmentSlot> slots = m_Slots;
		if (slots != null)
		{
			int val = 0;
			foreach (KeyValuePair<BlueprintAugmentSlot, AugmentSlot> item in slots)
			{
				Hash128 hash = default(Hash128);
				Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val2);
				Hash128 val3 = ClassHasher<AugmentSlot>.GetHash128(item.Value);
				hash.Append(ref val3);
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
		}
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_OverdriveSlotBp);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<Ability>.GetHash128(m_OverdriveAbility);
		result.Append(ref val5);
		return result;
	}
}
