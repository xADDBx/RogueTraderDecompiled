using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartShields : BaseUnitPart, IHashable
{
	public class Entry
	{
		[JsonIgnore]
		public string ShieldFactUid { get; set; }

		[JsonIgnore]
		public WarhammerCombatSide DefenceSide { get; set; }

		[JsonIgnore]
		public GameObject DefenceMarker { get; set; }

		[JsonIgnore]
		public WeakReference<AoEPattern> DefencePattern { get; set; }
	}

	[JsonIgnore]
	private Dictionary<string, Entry> m_Entries = new Dictionary<string, Entry>();

	public Entry Add(EntityFact sourceFact, [CanBeNull] EntityFact shieldFact, WarhammerCombatSide side, AoEPattern defencePattern)
	{
		if (shieldFact != null && m_Entries.TryGetValue(shieldFact.UniqueId, out var value))
		{
			return value;
		}
		Entry entry = new Entry
		{
			ShieldFactUid = (shieldFact?.UniqueId ?? ""),
			DefenceSide = side,
			DefencePattern = new WeakReference<AoEPattern>(defencePattern)
		};
		m_Entries.Add(sourceFact.UniqueId, entry);
		return entry;
	}

	public void Remove(EntityFact sourceFact)
	{
		m_Entries.Remove(sourceFact.UniqueId);
		if (m_Entries.Count <= 0)
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
		return result;
	}
}
