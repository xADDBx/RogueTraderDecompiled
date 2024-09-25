using System;
using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class ScatterRaysStrings : EnumStrings<RuleRollScatterShotHitDirection.Ray>
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

	public ScatterRaysStrings()
	{
		Entries = CreateEntries<MyEntry>();
	}

	protected override IEnumerable<Entry> GetEntries()
	{
		return Entries;
	}
}
