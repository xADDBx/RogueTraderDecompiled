using System;
using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class PregenPetNamesList
{
	public PetType PetType;

	public LocalizedString NameList;
}
