using System;
using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.UI;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIPropertyNames
{
	[Serializable]
	public class UIPropertyNameEntry
	{
		public UIPropertyName Type;

		public LocalizedString Text;
	}

	public List<UIPropertyNameEntry> Entries;
}
