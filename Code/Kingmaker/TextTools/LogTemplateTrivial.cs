using System;
using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class LogTemplateTrivial<T> : TextTemplate
{
	private readonly Func<T> m_Getter;

	private string Text => m_Getter().ToString();

	public LogTemplateTrivial(Func<T> getter)
	{
		m_Getter = getter;
	}

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!capitalized)
		{
			return Text;
		}
		return Text.ToUpperInvariant();
	}
}
