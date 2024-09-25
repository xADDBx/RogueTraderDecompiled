using System;
using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class LogTemplateDice<T> : TextTemplate where T : IRuleRollDice
{
	private readonly Func<T> m_Getter;

	private string Text => LogHelper.GetRollDescription(m_Getter());

	public LogTemplateDice(Func<T> getter)
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
