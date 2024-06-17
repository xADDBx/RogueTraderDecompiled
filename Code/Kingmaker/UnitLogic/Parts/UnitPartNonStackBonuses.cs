using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartNonStackBonuses : BaseUnitPart, IHashable
{
	private List<ModifiableValue.Modifier> m_NonStuckModifiers = new List<ModifiableValue.Modifier>();

	private Dictionary<ItemSlot, List<ModifiableValue.Modifier>> m_NonStuckSlots = new Dictionary<ItemSlot, List<ModifiableValue.Modifier>>();

	private Dictionary<Buff, List<ModifiableValue.Modifier>> m_NonStuckBuffs = new Dictionary<Buff, List<ModifiableValue.Modifier>>();

	public bool ShouldShowWarning(ItemSlot slot)
	{
		List<ModifiableValue.Modifier> nonStuckModifiers = GetNonStuckModifiers(slot);
		if (nonStuckModifiers != null)
		{
			return nonStuckModifiers.Count > 0;
		}
		return false;
	}

	public bool ShouldShowWarning(Buff buff)
	{
		List<ModifiableValue.Modifier> nonStuckModifiers = GetNonStuckModifiers(buff);
		if (nonStuckModifiers != null)
		{
			return nonStuckModifiers.Count > 0;
		}
		return false;
	}

	public List<ModifiableValue.Modifier> GetNonStuckModifiers(ItemSlot slot)
	{
		return m_NonStuckSlots.Get(slot);
	}

	public List<ModifiableValue.Modifier> GetNonStuckModifiers(Buff buff)
	{
		return m_NonStuckBuffs.Get(buff);
	}

	public List<ItemSlot> GetItemsList(ItemSlot excludeItem = null)
	{
		List<ItemSlot> list = new List<ItemSlot>();
		foreach (KeyValuePair<ItemSlot, List<ModifiableValue.Modifier>> nonStuckSlot in m_NonStuckSlots)
		{
			if (excludeItem != nonStuckSlot.Key)
			{
				list.Add(nonStuckSlot.Key);
			}
		}
		return list;
	}

	public List<Buff> GetBuffList(Buff excludeBuff = null)
	{
		List<Buff> list = new List<Buff>();
		foreach (KeyValuePair<Buff, List<ModifiableValue.Modifier>> nonStuckBuff in m_NonStuckBuffs)
		{
			if (excludeBuff != nonStuckBuff.Key)
			{
				list.Add(nonStuckBuff.Key);
			}
		}
		return list;
	}

	public void HandleModifierAdded(ModifiableValue modifiable, ModifiableValue.Modifier newMod)
	{
		if (!(modifiable.Owner is BaseUnitEntity baseUnitEntity) || !baseUnitEntity.Faction.IsPlayer || !baseUnitEntity.IsInCompanionRoster() || newMod.Stacks || newMod.ModValue <= 0 || (newMod.SourceItem == null && ((!(newMod.SourceFact?.MaybeContext?.MaybeCaster?.IsPlayerFaction)) ?? false)))
		{
			return;
		}
		bool flag = false;
		foreach (ModifiableValue.Modifier modifier in modifiable.GetModifiers(newMod.ModDescriptor))
		{
			if (modifier != newMod && !modifier.Stacks)
			{
				flag = true;
				AddNewModifier(newMod, modifier);
			}
		}
		if (flag)
		{
			m_NonStuckModifiers.Add(newMod);
			PFLog.EntityFact.Log("Add non-stack " + newMod);
			EventBus.RaiseEvent(delegate(INonStackModifierHandler h)
			{
				h.HandleNonStackModifierAdded(this, modifiable, newMod);
			});
		}
	}

	public void HandleModifierRemoving(ModifiableValue modifiable, ModifiableValue.Modifier mod)
	{
		if (!m_NonStuckModifiers.Remove(mod))
		{
			return;
		}
		PFLog.EntityFact.Log("Remove non-stack " + mod);
		using PooledList<ModifiableValue.Modifier> pooledList = PooledList<ModifiableValue.Modifier>.Get();
		if (mod.SourceFact is Buff key && m_NonStuckBuffs.TryGetValue(key, out var value))
		{
			pooledList.AddRange(value);
			ListPool<ModifiableValue.Modifier>.Release(value);
			m_NonStuckBuffs.Remove(key);
		}
		ItemSlot itemSlot = mod.SourceItem?.HoldingSlot;
		if (itemSlot != null && m_NonStuckSlots.TryGetValue(itemSlot, out var value2))
		{
			pooledList.AddRange(value2);
			value2.Clear();
		}
		foreach (ModifiableValue.Modifier item in pooledList)
		{
			if (item.SourceFact is Buff key2)
			{
				List<ModifiableValue.Modifier> list = m_NonStuckBuffs.Get(key2);
				if (list != null)
				{
					list.Remove(mod);
					if (list.Count == 0)
					{
						m_NonStuckBuffs.Remove(key2);
						m_NonStuckModifiers.Remove(item);
					}
				}
			}
			ItemSlot itemSlot2 = item.SourceItem?.HoldingSlot;
			if (itemSlot2 == null)
			{
				continue;
			}
			List<ModifiableValue.Modifier> list2 = m_NonStuckSlots.Get(itemSlot2);
			if (list2 != null)
			{
				list2.Remove(mod);
				if (list2.Count == 0)
				{
					m_NonStuckModifiers.Remove(item);
				}
			}
		}
	}

	private void AddNewModifier(ModifiableValue.Modifier newModifier, ModifiableValue.Modifier modifier)
	{
		if (!m_NonStuckModifiers.Contains(modifier))
		{
			m_NonStuckModifiers.Add(modifier);
		}
		AddModifierToSourceCollection(newModifier, modifier);
		AddModifierToSourceCollection(modifier, newModifier);
	}

	private void AddModifierToSourceCollection(ModifiableValue.Modifier collectionProvider, ModifiableValue.Modifier modifier)
	{
		if (collectionProvider.SourceFact is Buff key)
		{
			if (!m_NonStuckBuffs.TryGetValue(key, out var value))
			{
				value = ListPool<ModifiableValue.Modifier>.Claim();
				m_NonStuckBuffs.Add(key, value);
			}
			if (!value.Contains(modifier))
			{
				value.Add(modifier);
			}
			return;
		}
		ItemSlot itemSlot = collectionProvider.SourceItem?.HoldingSlot;
		if (itemSlot != null)
		{
			if (!m_NonStuckSlots.TryGetValue(itemSlot, out var value2))
			{
				value2 = ListPool<ModifiableValue.Modifier>.Claim();
				m_NonStuckSlots.Add(itemSlot, value2);
			}
			if (!value2.Contains(modifier))
			{
				value2.Add(modifier);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
