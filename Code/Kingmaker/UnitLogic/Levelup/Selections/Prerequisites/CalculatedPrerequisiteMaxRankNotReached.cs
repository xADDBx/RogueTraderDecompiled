using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;

namespace Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;

public class CalculatedPrerequisiteMaxRankNotReached : CalculatedPrerequisite
{
	public int MaxRank { get; }

	private static CalculatedPrerequisiteStrings Strings => LocalizedTexts.Instance.CalculatedPrerequisites;

	public CalculatedPrerequisiteMaxRankNotReached(bool value, int maxRank)
		: base(value)
	{
		MaxRank = maxRank;
	}

	protected override string GetDescriptionInternal()
	{
		return string.Format(Strings.CurrentRankLessThan.Text, MaxRank);
	}
}
