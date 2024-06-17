using System;
using System.Collections.Generic;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIProfitFactorTexts
{
	[Serializable]
	public class ProfitFactorSourcePair
	{
		public ProfitFactorModifierType SourceType;

		public LocalizedString SourceName;
	}

	public LocalizedString Title;

	public LocalizedString Description;

	public LocalizedString Initial;

	public LocalizedString TotalValue;

	public LocalizedString Income;

	public LocalizedString Loss;

	public LocalizedString AvailableToUseValue;

	public LocalizedString NoSourcesDesc;

	public LocalizedString ProfitFatorGainedNotification;

	public LocalizedString ProfitFatorLostNotification;

	public List<ProfitFactorSourcePair> Sources;

	public LocalizedString GetSource(ProfitFactorModifierType sourceType)
	{
		return Sources.FirstOrDefault((ProfitFactorSourcePair pair) => pair.SourceType == sourceType)?.SourceName;
	}
}
