using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[CreateAssetMenu(menuName = "Localization/CalculatedPrerequisite Strings")]
public class CalculatedPrerequisiteStrings : StringsContainer
{
	[Header("CalculatedPrerequisiteComposite")]
	public LocalizedString CompositeAndTrue;

	public LocalizedString CompositeAndFalse;

	public LocalizedString CompositeOrTrue;

	public LocalizedString CompositeOrFalse;

	[Header("CalculatedPrerequisiteFact")]
	public LocalizedString HasFact;

	public LocalizedString HasNoFact;

	public LocalizedString AtRankOrGreater;

	public LocalizedString AtLeastAtRank;

	[Header("CalculatedPrerequisiteMaxRankNotReached")]
	public LocalizedString CurrentRankLessThan;

	[Header("CalculatedPrerequisiteStat")]
	public LocalizedString HasStat;

	public LocalizedString StatLessThan;

	public LocalizedString StatGreaterOrEqual;
}
