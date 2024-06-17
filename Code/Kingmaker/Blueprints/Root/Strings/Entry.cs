using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class Entry
{
	public StatType Stat;

	public LocalizedString Text;

	public LocalizedString ShortText;

	public LocalizedString BonusText;
}
