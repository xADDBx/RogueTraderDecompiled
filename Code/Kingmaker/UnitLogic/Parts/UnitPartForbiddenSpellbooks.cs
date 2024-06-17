using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartForbiddenSpellbooks : BaseUnitPart, IHashable
{
	public class ForbiddenSpellbookEntry : IHashable
	{
		[JsonProperty]
		public readonly BlueprintSpellbook Spellbook;

		[JsonProperty]
		public readonly ForbidSpellbookReason Reason;

		[JsonConstructor]
		public ForbiddenSpellbookEntry(BlueprintSpellbook spellbook, ForbidSpellbookReason reason)
		{
			Spellbook = spellbook;
			Reason = reason;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Spellbook);
			result.Append(ref val);
			ForbidSpellbookReason val2 = Reason;
			result.Append(ref val2);
			return result;
		}
	}

	[NotNull]
	[JsonProperty]
	private List<ForbiddenSpellbookEntry> m_Entries = new List<ForbiddenSpellbookEntry>();

	public void Add(BlueprintSpellbook spellbook, ForbidSpellbookReason reason)
	{
		m_Entries.Add(new ForbiddenSpellbookEntry(spellbook, reason));
	}

	public void Remove(BlueprintSpellbook spellbook, ForbidSpellbookReason reason)
	{
		ForbiddenSpellbookEntry forbiddenSpellbookEntry = m_Entries.FirstOrDefault((ForbiddenSpellbookEntry e) => e.Spellbook == spellbook && e.Reason == reason);
		if (forbiddenSpellbookEntry != null)
		{
			m_Entries.Remove(forbiddenSpellbookEntry);
		}
		if (m_Entries.Count <= 0)
		{
			base.Owner.Remove<UnitPartForbiddenSpellbooks>();
		}
	}

	public bool IsForbidden(BlueprintSpellbook spellbook)
	{
		return m_Entries.Any((ForbiddenSpellbookEntry e) => e.Spellbook == spellbook);
	}

	public string GetReason(BlueprintSpellbook spellbook)
	{
		ForbiddenSpellbookEntry forbiddenSpellbookEntry = m_Entries.FirstOrDefault((ForbiddenSpellbookEntry e) => e.Spellbook == spellbook);
		if (forbiddenSpellbookEntry == null)
		{
			return "";
		}
		return forbiddenSpellbookEntry.Reason switch
		{
			ForbidSpellbookReason.Alignment => LocalizedTexts.Instance.Reasons.SpellbookForbiddenAlignment, 
			ForbidSpellbookReason.Armor => LocalizedTexts.Instance.Reasons.SpellbookForbiddenArmor, 
			ForbidSpellbookReason.Other => LocalizedTexts.Instance.Reasons.SpellbookForbiddenArmor, 
			_ => "", 
		};
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<ForbiddenSpellbookEntry> entries = m_Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<ForbiddenSpellbookEntry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
