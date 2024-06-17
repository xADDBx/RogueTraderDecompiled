using System;
using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class WeaponCategoryEntry
{
	public WeaponCategory Category;

	public LocalizedString Text;
}
