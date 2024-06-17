using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class PregenCharacterNameList
{
	public Race Race;

	public Gender Gender;

	public LocalizedString NameList;
}
