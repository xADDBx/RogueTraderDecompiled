using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartBatteringBlast : BaseUnitPart, IAreaHandler, ISubscriber, IHashable
{
	[JsonProperty]
	public List<AbilityData> Entries = new List<AbilityData>();

	public void NewEntry(AbilityData entry)
	{
		Entries.Add(entry);
	}

	public void RemoveEntry(AbilityData entry)
	{
		Entries.RemoveAll((AbilityData p) => p == entry);
	}

	public void OnAreaBeginUnloading()
	{
		Entries.Clear();
	}

	public int CountEntries(AbilityData entry)
	{
		return Entries.FindAll((AbilityData p) => p == entry).Count;
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<AbilityData> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<AbilityData>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
