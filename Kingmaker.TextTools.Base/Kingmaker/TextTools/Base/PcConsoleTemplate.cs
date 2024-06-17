using System.Collections.Generic;

namespace Kingmaker.TextTools.Base;

public class PcConsoleTemplate : TextTemplate
{
	public override int MinParameters => 2;

	public override int MaxParameters => 2;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		int num = 0;
		if (parameters.Count > num)
		{
			return parameters[num];
		}
		return string.Empty;
	}
}
