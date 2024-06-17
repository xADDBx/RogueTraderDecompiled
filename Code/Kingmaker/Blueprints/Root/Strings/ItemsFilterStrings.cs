using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Root.Strings;

public class ItemsFilterStrings : StringsContainer
{
	[Serializable]
	public class SorterTypeEntry
	{
		public ItemsSorterType SorterType;

		public LocalizedString Text;
	}

	[Serializable]
	public class FilterTypeEntry
	{
		public ItemsFilterType FilterType;

		public LocalizedString Text;
	}

	[NotNull]
	public SorterTypeEntry[] SorterTypeEntries;

	[NotNull]
	public FilterTypeEntry[] FilterTypentries;

	[NonSerialized]
	[CanBeNull]
	private Dictionary<ItemsSorterType, LocalizedString> m_SorterTypeCache;

	[NonSerialized]
	[CanBeNull]
	private Dictionary<ItemsFilterType, LocalizedString> m_FilterTypeCache;

	public string GetText(ItemsSorterType type)
	{
		if (m_SorterTypeCache == null)
		{
			m_SorterTypeCache = new Dictionary<ItemsSorterType, LocalizedString>();
			SorterTypeEntries.ForEach(delegate(SorterTypeEntry e)
			{
				m_SorterTypeCache[e.SorterType] = e.Text;
			});
		}
		if (m_SorterTypeCache.TryGetValue(type, out var value))
		{
			return value;
		}
		return type.ToString();
	}

	public string GetText(ItemsFilterType type)
	{
		if (m_FilterTypeCache == null)
		{
			m_FilterTypeCache = new Dictionary<ItemsFilterType, LocalizedString>();
			FilterTypentries.ForEach(delegate(FilterTypeEntry e)
			{
				m_FilterTypeCache[e.FilterType] = e.Text;
			});
		}
		if (m_FilterTypeCache.TryGetValue(type, out var value))
		{
			return value;
		}
		return type.ToString();
	}
}
