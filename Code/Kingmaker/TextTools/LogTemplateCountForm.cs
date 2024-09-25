using System.Collections.Generic;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.TextTools;

public class LogTemplateCountForm : TextTemplate
{
	public override int MinParameters => 2;

	public override int MaxParameters => 2;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		int num = (((int)GameLogContext.Count != 1) ? 1 : 0);
		if (num < 0 || num >= parameters.Count)
		{
			return "";
		}
		return parameters[num];
	}
}
