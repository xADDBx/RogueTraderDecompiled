using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class ConditionsString : EnumStrings<UnitCondition>
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

	public ConditionsString()
	{
		Entries = CreateEntries<MyEntry>();
	}

	protected override IEnumerable<Entry> GetEntries()
	{
		return Entries;
	}
}
