using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartDamageGrace : BaseUnitPart, IHashable
{
	public class DamageGraceEntry
	{
		public WeaponCategory Category;

		public EntityFact Source;
	}

	public List<DamageGraceEntry> Weapons = new List<DamageGraceEntry>();

	public void AddEntry(WeaponCategory? category, EntityFact source)
	{
		if (category.HasValue)
		{
			DamageGraceEntry item = new DamageGraceEntry
			{
				Category = category.Value,
				Source = source
			};
			Weapons.Add(item);
		}
	}

	public void RemoveEntry(EntityFact source)
	{
		Weapons.RemoveAll((DamageGraceEntry p) => p.Source == source);
	}

	public bool HasEntry(WeaponCategory category)
	{
		return Weapons.Any((DamageGraceEntry p) => p.Category == category);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
