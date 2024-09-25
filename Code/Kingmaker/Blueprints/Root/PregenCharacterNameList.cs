using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.CharGen;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class PregenCharacterNameList
{
	public Race Race;

	public Gender Gender;

	public CharGenConfig.CharGenMode CharGenMode;

	public LocalizedString NameList;
}
