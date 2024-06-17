using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class TooltipEndTemplate : TextTemplate
{
	private TooltipType m_Type;

	public override int Balance => -1;

	public TooltipEndTemplate(TooltipType type)
	{
		m_Type = type;
	}

	public override string Generate(bool capitalized, List<string> parameters)
	{
		return "</link></color></b>";
	}
}
