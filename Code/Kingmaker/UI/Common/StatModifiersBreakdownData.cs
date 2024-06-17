using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.UI.Common;

public class StatModifiersBreakdownData
{
	[NotNull]
	internal readonly List<StatBonusEntry> Bonuses;

	public readonly List<StatBonusEntry> SortedBonuses;

	public bool HasBonuses => Bonuses.Count > 0;

	public StatModifiersBreakdownData([NotNull] List<StatBonusEntry> bonuses)
	{
		Bonuses = bonuses;
		SortedBonuses = bonuses;
		SortedBonuses.Sort(StatBonusEntry.Compare);
	}
}
