using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class PercentHelper
{
	public LocalizedString PercentTemplate;

	public string AddPercentTo(string label)
	{
		return string.Format(PercentTemplate, label);
	}

	public string AddPercentTo(int number)
	{
		return AddPercentTo(number.ToString());
	}
}
