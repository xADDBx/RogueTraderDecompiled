using System;
using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITooltipElementLabels
{
	[Serializable]
	public class UITooltipElementEntry
	{
		public TooltipElement Type;

		public LocalizedString Text;
	}

	public List<UITooltipElementEntry> Entries;

	public string GetLabel(TooltipElement type)
	{
		LocalizedString localizedString = Entries.FirstOrDefault((UITooltipElementEntry pred) => pred.Type == type)?.Text;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}
}
