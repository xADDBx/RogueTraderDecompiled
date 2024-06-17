using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class SpellDescriptorString : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		public SpellDescriptorWrapper Descriptor;

		public LocalizedString Text;
	}

	public Entry[] Entries;

	public SpellDescriptorString()
	{
		List<Entry> list = new List<Entry>();
		foreach (SpellDescriptor value in EnumUtils.GetValues<SpellDescriptor>())
		{
			Entry item = new Entry
			{
				Descriptor = value
			};
			list.Add(item);
		}
		Entries = list.ToArray();
	}

	public string GetText(SpellDescriptor descriptor)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Entry[] entries = Entries;
		foreach (Entry entry in entries)
		{
			if (!entry.Descriptor.Value.IsSystemSpellDescriptor() && descriptor.Intersects(entry.Descriptor.Value))
			{
				UIUtilityTexts.TryAddWordSeparator(stringBuilder, ", ");
				stringBuilder.Append(entry.Text);
			}
		}
		return stringBuilder.ToString();
	}
}
