using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI.Common.UIConfigComponents;

[TypeId("0e3077b972b14bd6943e8100d8ed9c92")]
public class EnumUnitSubtypeIcons : EnumSpritesBlueprint<UnitSubtype>
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

	public EnumUnitSubtypeIcons()
	{
		Entries = CreateEntries<MyEntry>();
	}

	protected override IEnumerable<Entry> GetEntries()
	{
		return Entries;
	}
}
