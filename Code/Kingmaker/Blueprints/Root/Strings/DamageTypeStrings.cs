using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[CreateAssetMenu(menuName = "Localization/DamageTypeStrings Strings")]
public class DamageTypeStrings : EnumStrings<DamageType>
{
	[Serializable]
	public class MyEntry : Entry, IHashable
	{
		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public MyEntry[] Entries;

	public DamageTypeStrings()
	{
		Entries = CreateEntries<MyEntry>();
	}

	protected override IEnumerable<Entry> GetEntries()
	{
		return Entries;
	}
}
