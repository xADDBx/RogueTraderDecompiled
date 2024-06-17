using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints;

[TypeId("fab4f1e3830ee8842ab3b77782add352")]
public class BlueprintMultiEntrance : BlueprintScriptableObject
{
	public enum BlueprintMultiEntranceMap
	{
		Footfall,
		Voidship
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintMultiEntrance>
	{
	}

	public BlueprintMultiEntranceMap Map;

	public LocalizedString Name;

	[SerializeField]
	private BlueprintMultiEntranceEntryReference[] m_Entries = new BlueprintMultiEntranceEntryReference[0];

	public ReferenceArrayProxy<BlueprintMultiEntranceEntry> Entries
	{
		get
		{
			BlueprintReference<BlueprintMultiEntranceEntry>[] entries = m_Entries;
			return entries;
		}
	}

	public BlueprintMultiEntranceEntry[] GetVisibleEntries()
	{
		return Entries.Where((BlueprintMultiEntranceEntry e) => e.IsVisible).ToArray();
	}
}
