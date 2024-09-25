using System;
using System.Collections.Generic;
using Kingmaker.Inspect;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class InspectPartString : EnumStrings<UnitInfoPart>
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

	public InspectPartString()
	{
		Entries = CreateEntries<MyEntry>();
	}

	protected override IEnumerable<Entry> GetEntries()
	{
		return Entries;
	}
}
