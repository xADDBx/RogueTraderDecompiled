using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;

public class CalculatedPrerequisiteFact : CalculatedPrerequisite
{
	public BlueprintUnitFact Fact { get; }

	public int MinRank { get; }

	private static CalculatedPrerequisiteStrings Strings => LocalizedTexts.Instance.CalculatedPrerequisites;

	public CalculatedPrerequisiteFact(bool value, [NotNull] PrerequisiteFact source)
		: base(value, source)
	{
		Fact = source.Fact;
		MinRank = source.MinRank;
	}

	protected override string GetDescriptionInternal()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		LocalizedString localizedString = (base.Not ? Strings.HasNoFact : Strings.HasFact);
		builder.Append(string.Format(localizedString.Text, Fact.Name));
		if (MinRank > 1)
		{
			builder.Append(base.Not ? string.Format(Strings.AtRankOrGreater.Text, MinRank) : string.Format(Strings.AtLeastAtRank.Text, MinRank));
		}
		return builder.ToString();
	}
}
