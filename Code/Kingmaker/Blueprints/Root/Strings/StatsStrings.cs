using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class StatsStrings : StringsContainer, ISerializationCallbackReceiver
{
	[NotNull]
	public Entry[] Entries = Array.Empty<Entry>();

	[NotNull]
	[SerializeField]
	private WeaponCategoryEntry[] WeaponCategoryEntries = Array.Empty<WeaponCategoryEntry>();

	[NotNull]
	[SerializeField]
	private WeaponFamilyEntry[] WeaponFamilyEntries = Array.Empty<WeaponFamilyEntry>();

	[NotNull]
	[SerializeField]
	private ArmorEntry[] ArmorEntries = Array.Empty<ArmorEntry>();

	[NotNull]
	[SerializeField]
	private ArmorCategoryEntry[] ArmorCategoryEntriesEntries = Array.Empty<ArmorCategoryEntry>();

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<StatType, LocalizedString> m_StatsCache = EmptyDictionary<StatType, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<StatType, LocalizedString> m_StatsShortCache = EmptyDictionary<StatType, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<StatType, LocalizedString> m_StatsBonusCache = EmptyDictionary<StatType, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<WeaponCategory, LocalizedString> m_WeaponCategoryCache = EmptyDictionary<WeaponCategory, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<WeaponFamily, LocalizedString> m_WeaponFamilyCache = EmptyDictionary<WeaponFamily, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<ArmorProficiencyGroup, LocalizedString> m_ArmorCache = EmptyDictionary<ArmorProficiencyGroup, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<WarhammerArmorCategory, LocalizedString> m_ArmorCategoryCache = EmptyDictionary<WarhammerArmorCategory, LocalizedString>.Instance;

	public string GetText(StatType stat)
	{
		if (!m_StatsCache.TryGetValue(stat, out var value))
		{
			return stat.ToString();
		}
		return value;
	}

	public string GetShortText(StatType stat)
	{
		if (!m_StatsShortCache.TryGetValue(stat, out var value))
		{
			return GetText(stat);
		}
		return value;
	}

	public string GetBonusText(StatType stat)
	{
		if (!m_StatsBonusCache.TryGetValue(stat, out var value))
		{
			return GetText(stat);
		}
		return value;
	}

	public string GetText(WeaponCategory stat)
	{
		if (!m_WeaponCategoryCache.TryGetValue(stat, out var value))
		{
			return UIStrings.Instance.WeaponCategories.GetWeaponCategoryLabel(stat);
		}
		return value;
	}

	public string GetText(WeaponFamily stat)
	{
		if (!m_WeaponFamilyCache.TryGetValue(stat, out var value))
		{
			return UIStrings.Instance.WeaponCategories.GetWeaponFamilyLabel(stat);
		}
		return value;
	}

	public string GetText(ArmorProficiencyGroup stat)
	{
		if (!m_ArmorCache.TryGetValue(stat, out var value))
		{
			return stat.ToString();
		}
		return value;
	}

	public string GetText(WarhammerArmorCategory category)
	{
		if (!m_ArmorCategoryCache.TryGetValue(category, out var value))
		{
			return category.ToString();
		}
		return value;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		m_WeaponCategoryCache = WeaponCategoryEntries.Distinct(EqualitySelector.Create((WeaponCategoryEntry v) => v.Category)).ToDictionary((WeaponCategoryEntry v) => v.Category, (WeaponCategoryEntry v) => v.Text);
		m_WeaponFamilyCache = WeaponFamilyEntries.Distinct(EqualitySelector.Create((WeaponFamilyEntry v) => v.Family)).ToDictionary((WeaponFamilyEntry v) => v.Family, (WeaponFamilyEntry v) => v.Text);
		m_ArmorCache = ArmorEntries.Distinct(EqualitySelector.Create((ArmorEntry v) => v.Proficiency)).ToDictionary((ArmorEntry v) => v.Proficiency, (ArmorEntry v) => v.Text);
		m_ArmorCategoryCache = ArmorCategoryEntriesEntries.Distinct(EqualitySelector.Create((ArmorCategoryEntry v) => v.Category)).ToDictionary((ArmorCategoryEntry v) => v.Category, (ArmorCategoryEntry v) => v.Text);
		m_StatsCache = Entries.Distinct(EqualitySelector.Create((Entry v) => v.Stat)).ToDictionary((Entry v) => v.Stat, (Entry v) => v.Text);
		m_StatsShortCache = Entries.Distinct(EqualitySelector.Create((Entry v) => v.Stat)).ToDictionary((Entry v) => v.Stat, (Entry v) => v.ShortText);
		m_StatsBonusCache = Entries.Distinct(EqualitySelector.Create((Entry v) => v.Stat)).ToDictionary((Entry v) => v.Stat, (Entry v) => v.BonusText);
	}
}
