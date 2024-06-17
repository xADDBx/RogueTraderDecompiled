using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartModifiedSpells : BaseUnitPart, IHashable
{
	public class ModifiedSpell
	{
		public BlueprintAbility Spell;

		public EntityFact Source;

		public SpellModificationType Modification;
	}

	public List<ModifiedSpell> Entries = new List<ModifiedSpell>();

	public void AddEntry(BlueprintAbility spell, EntityFact source, SpellModificationType modification)
	{
		ModifiedSpell item = new ModifiedSpell
		{
			Spell = spell,
			Source = source,
			Modification = modification
		};
		Entries.Add(item);
	}

	public void RemoveEntry(EntityFact source)
	{
		Entries.RemoveAll((ModifiedSpell p) => p.Source == source);
	}

	public bool HasEntry(BlueprintAbility spell, SpellModificationType modification)
	{
		return Entries.Any((ModifiedSpell p) => p.Spell == spell && p.Modification == modification);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
