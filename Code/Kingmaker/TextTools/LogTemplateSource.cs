using System.Collections.Generic;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.TextTools;

public class LogTemplateSource : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		object obj;
		if (string.IsNullOrEmpty(GameLogContext.OverrideName.Value))
		{
			if (GameLogContext.SourceEntity.Value != null)
			{
				return LogHelper.GetEntityName(GameLogContext.SourceEntity.Value);
			}
			obj = GameLogContext.SourceFact.Value?.Name;
			if (obj == null)
			{
				return "";
			}
		}
		else
		{
			obj = LogHelper.GetOverrideName(GameLogContext.OverrideName.Value, GameLogContext.OverrideNameColor.Value);
		}
		return (string)obj;
	}
}
