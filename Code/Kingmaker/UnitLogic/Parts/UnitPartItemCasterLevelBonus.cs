using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartItemCasterLevelBonus : BaseUnitPart, IHashable
{
	public class CasterLevelBonusEntry
	{
		public int Bonus;

		public EntityFact Source;

		public UsableItemType ItemType;
	}

	public List<CasterLevelBonusEntry> Entries = new List<CasterLevelBonusEntry>();

	public void AddEntry(int bonus, UsableItemType itemType, EntityFact source)
	{
		CasterLevelBonusEntry item = new CasterLevelBonusEntry
		{
			Bonus = bonus,
			Source = source,
			ItemType = itemType
		};
		Entries.Add(item);
	}

	public void RemoveEntry(EntityFact source)
	{
		Entries.RemoveAll((CasterLevelBonusEntry p) => p.Source == source);
	}

	public int GetBonus(UsableItemType? itemType)
	{
		int num = 0;
		foreach (CasterLevelBonusEntry item in Entries.Where((CasterLevelBonusEntry p) => p.ItemType == itemType))
		{
			num += item.Bonus;
		}
		return num;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
