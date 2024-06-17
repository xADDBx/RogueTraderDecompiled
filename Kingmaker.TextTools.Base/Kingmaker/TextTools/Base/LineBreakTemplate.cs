using System.Collections.Generic;

namespace Kingmaker.TextTools.Base;

public class LineBreakTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return "\n";
	}
}
