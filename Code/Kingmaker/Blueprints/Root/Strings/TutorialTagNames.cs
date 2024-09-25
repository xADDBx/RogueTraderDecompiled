using System;
using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.Tutorial;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class TutorialTagNames
{
	[Serializable]
	public class TutorialTagPair
	{
		public TutorialTag Tag;

		public LocalizedString Name;
	}

	public List<TutorialTagPair> Names;

	public LocalizedString GetTagName(TutorialTag tag)
	{
		return Names.FirstOrDefault((TutorialTagPair pair) => pair.Tag == tag)?.Name;
	}
}
