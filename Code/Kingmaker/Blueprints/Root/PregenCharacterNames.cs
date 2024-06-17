using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Base;
using Kingmaker.Localization;
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

	public string GetRandomName(Race race, Gender gender)
	{
		return (m_CharacterNames.FirstOrDefault((PregenCharacterNameList ch) => ch.Race == race && ch.Gender == gender)?.NameList)?.Text.Split(new char[5] { '.', ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries).Random(PFStatefulRandom.NonDeterministic);
	}

	public string GetRandomPetName()
	{
		return m_PetNames.Text.Split(new char[5] { '.', ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries).Random(PFStatefulRandom.Blueprints);
	}

	public string GetRandomShipName()
	{
		return m_ShipNames.Text.Split(new char[5] { '.', ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries).Random(PFStatefulRandom.NonDeterministic);
	}
}
