using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Root.Strings;

public class SizeStrings : StringsContainer
{
	[NotNull]
	public SizeEntry[] Entries;

	[NonSerialized]
	[CanBeNull]
	private Dictionary<Size, LocalizedString> m_Cache;

	public string GetText(Size size)
	{
		if (m_Cache == null)
		{
			m_Cache = new Dictionary<Size, LocalizedString>();
			Entries.ForEach(delegate(SizeEntry e)
			{
				m_Cache[e.Size] = e.Text;
			});
		}
		if (m_Cache.TryGetValue(size, out var value))
		{
			return value;
		}
		return size.ToString();
	}
}
