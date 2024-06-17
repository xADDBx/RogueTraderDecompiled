using System.Collections.Generic;

namespace Kingmaker.TextTools.Base;

public class EmptyTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return "";
	}
}
