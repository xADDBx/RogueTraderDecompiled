using System;
using System.Collections.Generic;
using Kingmaker.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[CreateAssetMenu(menuName = "Localization/WeaponCategoryString Strings")]
public class WeaponCategoryString : EnumStrings<WeaponCategory>
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

	public WeaponCategoryString()
	{
		Entries = CreateEntries<MyEntry>();
	}

	protected override IEnumerable<Entry> GetEntries()
	{
		return Entries;
	}
}
