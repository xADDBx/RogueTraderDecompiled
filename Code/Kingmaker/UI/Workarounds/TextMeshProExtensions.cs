using System.Collections.Generic;
using TMPro;

namespace Kingmaker.UI.Workarounds;

public static class TextMeshProExtensions
{
	public static void AddOptions(this TMP_Dropdown dropdown, IReadOnlyList<string> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			dropdown.options.Add(new TMP_Dropdown.OptionData(options[i]));
		}
		dropdown.RefreshShownValue();
	}
}
