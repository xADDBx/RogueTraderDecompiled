using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorRandomPhrases : StringsContainer
{
	[Header("Drusians")]
	public List<LocalizedString> HelloPhrasesDrusians;

	public List<LocalizedString> FinishDealPhrasesDrusians;

	[Header("Explorators")]
	public List<LocalizedString> HelloPhrasesExplorators;

	public List<LocalizedString> FinishDealPhrasesExplorators;

	[Header("Kasballica")]
	public List<LocalizedString> HelloPhrasesKasballica;

	public List<LocalizedString> FinishDealPhrasesKasballica;

	[Header("Pirates")]
	public List<LocalizedString> HelloPhrasesPirates;

	public List<LocalizedString> FinishDealPhrasesPirates;

	[Header("ShipVendor")]
	public List<LocalizedString> HelloPhrasesShipVendor;

	public List<LocalizedString> FinishDealPhrasesShipVendor;

	[Header("Special Vendors Phrases")]
	public SpecialPhrasesDict[] SpecialPhrases;

	private int m_Index;

	public string TakePhrase(FactionType factionType, BaseUnitEntity unit)
	{
		SpecialPhrasesDict[] specialPhrases = SpecialPhrases;
		foreach (SpecialPhrasesDict specialPhrasesDict in specialPhrases)
		{
			if (specialPhrasesDict.Vendors.Any((BlueprintUnitReference x) => x.Is(unit?.Blueprint)))
			{
				m_Index = Random.Range(0, specialPhrasesDict.HelloPhrases.Count);
				return specialPhrasesDict.HelloPhrases[m_Index];
			}
		}
		List<LocalizedString> list = null;
		switch (factionType)
		{
		case FactionType.Drusians:
			list = HelloPhrasesDrusians;
			break;
		case FactionType.Explorators:
			list = HelloPhrasesExplorators;
			break;
		case FactionType.Kasballica:
			list = HelloPhrasesKasballica;
			break;
		case FactionType.Pirates:
			list = HelloPhrasesPirates;
			break;
		case FactionType.ShipVendor:
			list = HelloPhrasesShipVendor;
			break;
		default:
			return string.Empty;
		}
		m_Index = Random.Range(0, list.Count);
		return list[m_Index];
	}

	public string TakeFinishDealPhrase(FactionType factionType, BaseUnitEntity unit)
	{
		SpecialPhrasesDict[] specialPhrases = SpecialPhrases;
		foreach (SpecialPhrasesDict specialPhrasesDict in specialPhrases)
		{
			if (specialPhrasesDict.Vendors.Any((BlueprintUnitReference x) => x.Is(unit.Blueprint)))
			{
				m_Index = Random.Range(0, specialPhrasesDict.FinishDealPhrases.Count);
				return specialPhrasesDict.FinishDealPhrases[m_Index];
			}
		}
		List<LocalizedString> list = null;
		switch (factionType)
		{
		case FactionType.Drusians:
			list = FinishDealPhrasesDrusians;
			break;
		case FactionType.Explorators:
			list = FinishDealPhrasesExplorators;
			break;
		case FactionType.Kasballica:
			list = FinishDealPhrasesKasballica;
			break;
		case FactionType.Pirates:
			list = FinishDealPhrasesPirates;
			break;
		case FactionType.ShipVendor:
			list = FinishDealPhrasesShipVendor;
			break;
		}
		m_Index = Random.Range(0, list.Count);
		return list[m_Index];
	}
}
