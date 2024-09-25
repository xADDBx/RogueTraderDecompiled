using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.UI.Common;

public class UITemplatePartraitsPath : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return UIUtilityTexts.GetPortraitsPath();
	}
}
