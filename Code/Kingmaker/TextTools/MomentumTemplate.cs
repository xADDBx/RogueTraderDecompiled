using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class MomentumTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return GameLogStrings.Instance.MomentumColorText.GetColorText();
	}
}
