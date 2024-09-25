using System;
using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class WeaponFamilyEntry
{
	public WeaponFamily Family;

	public LocalizedString Text;
}
