using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartUnlimitedSpells : BaseUnitPart, IHashable
{
	public class UnlimitedEntry
	{
		public BlueprintAbility Ability;

		public EntityFact Source;
	}

	public List<UnlimitedEntry> Entries = new List<UnlimitedEntry>();

	public void AddEntry(BlueprintAbility ability, EntityFact source)
	{
		UnlimitedEntry item = new UnlimitedEntry
		{
			Ability = ability,
			Source = source
		};
		Entries.Add(item);
	}

	public void RemoveUnlimitedEntry(EntityFact source)
	{
		Entries.RemoveAll((UnlimitedEntry p) => p.Source == source);
	}

	public bool CheckUnlimitedEntry(BlueprintAbility ability)
	{
		return Entries.Any((UnlimitedEntry p) => p.Ability == ability);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
