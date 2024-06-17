using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartExpandedArsenal : BaseUnitPart, IHashable
{
	public class SpellSchoolEntry
	{
		public SpellSchool School;

		public EntityFact Source;
	}

	public List<SpellSchoolEntry> Entries = new List<SpellSchoolEntry>();

	public void AddSpellSchoolEntry(SpellSchool school, EntityFact source)
	{
		SpellSchoolEntry item = new SpellSchoolEntry
		{
			School = school,
			Source = source
		};
		Entries.Add(item);
	}

	public void RemoveSpellSchoolEntry(EntityFact source)
	{
		Entries.RemoveAll((SpellSchoolEntry p) => p.Source == source);
	}

	public bool HasSpellSchoolEntry(SpellSchool school)
	{
		return Entries.Any((SpellSchoolEntry p) => p.School == school);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
