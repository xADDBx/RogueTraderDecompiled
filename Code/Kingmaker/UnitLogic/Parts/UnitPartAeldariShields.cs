using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartAeldariShields : BaseUnitPart, IHashable
{
	public class Entry : IHashable
	{
		[JsonProperty]
		public int CurrentNumberOfCharges { get; set; }

		[JsonProperty]
		public int ChargesUsedRound { get; set; }

		[JsonIgnore]
		public int BlockChance { get; set; }

		[JsonIgnore]
		public int NumberOfCharges { get; set; }

		[JsonIgnore]
		public int ResetChargeCooldownInRounds { get; set; }

		public bool ResetNumberOfCharges(int currentRound)
		{
			if (CurrentNumberOfCharges < NumberOfCharges && currentRound - ResetChargeCooldownInRounds >= ChargesUsedRound)
			{
				ChargesUsedRound = 0;
				CurrentNumberOfCharges = NumberOfCharges;
				return true;
			}
			return false;
		}

		public void SpendCharge(int currentRound)
		{
			if (CurrentNumberOfCharges == NumberOfCharges)
			{
				ChargesUsedRound = currentRound;
			}
			CurrentNumberOfCharges = Mathf.Max(CurrentNumberOfCharges - 1, 0);
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			int val = CurrentNumberOfCharges;
			result.Append(ref val);
			int val2 = ChargesUsedRound;
			result.Append(ref val2);
			return result;
		}
	}

	[JsonProperty]
	private Dictionary<string, Entry> m_Entries = new Dictionary<string, Entry>();

	public IEnumerable<Tuple<string, Entry>> ActiveEntries()
	{
		foreach (KeyValuePair<string, Entry> entry in m_Entries)
		{
			if (entry.Value.CurrentNumberOfCharges > 0)
			{
				yield return Tuple.Create(entry.Key, entry.Value);
			}
		}
	}

	public Entry Add(EntityFact sourceFact, int numberOfCharges, int blockChance, int resetChargeCooldownInRounds)
	{
		string uniqueId = sourceFact.UniqueId;
		if (!m_Entries.TryGetValue(uniqueId, out var value))
		{
			value = new Entry
			{
				ChargesUsedRound = 0,
				CurrentNumberOfCharges = numberOfCharges
			};
			m_Entries.Add(uniqueId, value);
		}
		value.BlockChance = blockChance;
		value.NumberOfCharges = numberOfCharges;
		value.ResetChargeCooldownInRounds = resetChargeCooldownInRounds;
		return value;
	}

	public bool Remove(EntityFact sourceFact)
	{
		bool result = m_Entries.Remove(sourceFact.UniqueId);
		if (m_Entries.Count <= 0)
		{
			RemoveSelf();
		}
		return result;
	}

	public Entry Get(EntityFact sourceFact)
	{
		return m_Entries.GetValueOrDefault(sourceFact.UniqueId);
	}

	public bool TryGet(EntityFact sourceFact, out Entry entry)
	{
		return m_Entries.TryGetValue(sourceFact.UniqueId, out entry);
	}

	public bool Has(EntityFact sourceFact)
	{
		return m_Entries.ContainsKey(sourceFact.UniqueId);
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
