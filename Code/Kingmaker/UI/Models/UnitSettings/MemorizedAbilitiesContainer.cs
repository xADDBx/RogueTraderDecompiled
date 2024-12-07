using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

[Serializable]
internal class MemorizedAbilitiesContainer : IHashable
{
	internal struct MemorizedAbilityData : IHashable
	{
		[JsonProperty]
		public BlueprintUnitFact ability;

		[JsonProperty]
		public ItemEntity sourceItem;

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(ability);
			result.Append(ref val);
			Hash128 val2 = ClassHasher<ItemEntity>.GetHash128(sourceItem);
			result.Append(ref val2);
			return result;
		}
	}

	[JsonProperty]
	private HashSet<MemorizedAbilityData> m_Memorized = new HashSet<MemorizedAbilityData>();

	internal void Add(BlueprintUnitFact ability, ItemEntity sourceItem)
	{
		m_Memorized.Add(new MemorizedAbilityData
		{
			ability = ability,
			sourceItem = sourceItem
		});
	}

	internal bool Contains(BlueprintUnitFact ability, ItemEntity sourceItem)
	{
		return m_Memorized.Contains(new MemorizedAbilityData
		{
			ability = ability,
			sourceItem = sourceItem
		});
	}

	internal bool Remove(BlueprintUnitFact ability, ItemEntity sourceItem)
	{
		return m_Memorized.Remove(new MemorizedAbilityData
		{
			ability = ability,
			sourceItem = sourceItem
		});
	}

	internal void IntersectWith(HashSet<MemorizedAbilityData> slots)
	{
		m_Memorized.IntersectWith(slots);
	}

	internal int Count()
	{
		return m_Memorized.Count;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		HashSet<MemorizedAbilityData> memorized = m_Memorized;
		if (memorized != null)
		{
			int num = 0;
			foreach (MemorizedAbilityData item in memorized)
			{
				MemorizedAbilityData obj = item;
				num ^= StructHasher<MemorizedAbilityData>.GetHash128(ref obj).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}
}
