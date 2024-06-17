using System.Collections.Generic;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.TextTools;

public class LogTemplateSource : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		object obj;
		if (GameLogContext.SourceEntity.Value == null)
		{
			obj = GameLogContext.SourceFact.Value?.Name;
			if (obj == null)
			{
				return "";
			}
		}
		else
		{
			obj = LogHelper.GetEntityName(GameLogContext.SourceEntity.Value);
		}
		return (string)obj;
	}
}
