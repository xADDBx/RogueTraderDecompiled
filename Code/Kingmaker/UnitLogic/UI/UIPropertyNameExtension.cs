using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;

namespace Kingmaker.UnitLogic.UI;

public static class UIPropertyNameExtension
{
	public static string GetLocalizedName(this UIPropertyName name)
	{
		LocalizedString localizedString = UIStrings.Instance.UIPropertyNames.Entries.FirstOrDefault((UIPropertyNames.UIPropertyNameEntry pred) => pred.Type == name)?.Text;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}
}
