using System.Collections.Generic;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.TextTools;

public class MomentumValue : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return GameLogContext.MomentumValue.ToString();
	}
}
