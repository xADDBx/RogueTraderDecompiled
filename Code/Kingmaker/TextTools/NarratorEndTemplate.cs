using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class NarratorEndTemplate : TextTemplate
{
	public override int Balance => -1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		return "</color></i>";
	}
}
