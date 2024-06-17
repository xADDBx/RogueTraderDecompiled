using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Parts;

public class StarshipPartBlockedComponents : BaseUnitPart, IHashable
{
	public class Entry : IHashable
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? ComponentIndex { get; set; }

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			if (ComponentIndex.HasValue)
			{
				int val = ComponentIndex.Value;
				result.Append(ref val);
			}
			return result;
		}
	}

	[JsonProperty]
	private readonly Dictionary<string, Entry> m_Entries = new Dictionary<string, Entry>();

	public void Block(EntityFact sourceFact, int componentIndex)
	{
		WeaponSlot weaponSlot = base.Owner.GetHull()?.WeaponSlots.Get(componentIndex);
		if (weaponSlot != null)
		{
			weaponSlot.Block();
			Entry value = new Entry
			{
				ComponentIndex = componentIndex
			};
			m_Entries.Add(sourceFact.UniqueId, value);
		}
	}

	public void Unblock(EntityFact sourceFact)
	{
		Entry entry = m_Entries.Get(sourceFact.UniqueId);
		((!entry.ComponentIndex.HasValue) ? null : base.Owner.GetHull()?.WeaponSlots.Get(entry.ComponentIndex.Value))?.Unblock();
		m_Entries.Remove(sourceFact.UniqueId);
		if (m_Entries.Empty())
		{
			RemoveSelf();
		}
	}

	[CanBeNull]
	public Entry Get(EntityFact sourceFact)
	{
		return m_Entries.Get(sourceFact.UniqueId);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<string, Entry> entries = m_Entries;
		if (entries != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<string, Entry> item in entries)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = StringHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<Entry>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		return result;
	}
}
