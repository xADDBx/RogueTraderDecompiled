using System;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class ArmorEntry
{
	public ArmorProficiencyGroup Proficiency;

	public LocalizedString Text;
}
