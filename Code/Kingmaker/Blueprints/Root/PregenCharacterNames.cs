using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Base;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[CreateAssetMenu(menuName = "Blueprints/CharGen/PregenCharacterNames")]
public class PregenCharacterNames : ScriptableObject
{
	[SerializeField]
	private List<PregenCharacterNameList> m_CharacterNames;

	[SerializeField]
	private LocalizedString m_PetNames;

	[SerializeField]
	private LocalizedString m_ShipNames;

	private List<string> GetNamesList(Race race, Gender gender, CharGenConfig.CharGenMode mode, string exceptName)
	{
		return GetNamesFromStringExcept((m_CharacterNames.FirstOrDefault((PregenCharacterNameList ch) => ch.Race == race && ch.Gender == gender && ch.CharGenMode == mode)?.NameList)?.Text, exceptName);
	}

	public string GetRandomName(Race race, Gender gender, CharGenConfig.CharGenMode mode, string exceptName)
	{
		return GetNamesList(race, gender, mode, exceptName).Random(PFStatefulRandom.NonDeterministic);
	}

	public string GetDefaultName(Race race, Gender gender, CharGenConfig.CharGenMode mode, string exceptName)
	{
		return GetNamesList(race, gender, mode, exceptName).FirstOrDefault();
	}

	public string GetRandomPetName(string exceptName)
	{
		return GetNamesFromStringExcept(m_PetNames.Text, exceptName).Random(PFStatefulRandom.Blueprints);
	}

	public string GetRandomShipName(string exceptName)
	{
		return GetNamesFromStringExcept(m_ShipNames.Text, exceptName).Random(PFStatefulRandom.NonDeterministic);
	}

	public string GetDefaultShipName(string exceptName)
	{
		return GetNamesFromStringExcept(m_ShipNames.Text, exceptName).FirstOrDefault();
	}

	private List<string> GetNamesFromStringExcept(string namesString, string exceptName)
	{
		List<string> list = namesString.Split(new char[5] { '.', ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
		if (!string.IsNullOrEmpty(exceptName))
		{
			list.RemoveAll((string n) => n == exceptName);
		}
		if (!list.Any())
		{
			return new List<string> { exceptName };
		}
		return list;
	}

	public string ReplaceCharacterNameIfCustom(Race race, Gender gender, CharGenConfig.CharGenMode mode, string characterName)
	{
		List<string> namesList = GetNamesList(race, gender, mode, string.Empty);
		if (!namesList.Contains(characterName))
		{
			return namesList.Random(PFStatefulRandom.NonDeterministic);
		}
		return characterName;
	}

	public string ReplaceShipNameIfCustom(string shipName)
	{
		List<string> namesFromStringExcept = GetNamesFromStringExcept(m_ShipNames.Text, string.Empty);
		if (!namesFromStringExcept.Contains(shipName))
		{
			return namesFromStringExcept.Random(PFStatefulRandom.NonDeterministic);
		}
		return shipName;
	}
}
