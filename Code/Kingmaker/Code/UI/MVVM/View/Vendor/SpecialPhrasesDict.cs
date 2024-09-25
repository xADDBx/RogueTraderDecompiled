using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Localization;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

[Serializable]
public class SpecialPhrasesDict
{
	public string Comment;

	public List<BlueprintUnitReference> Vendors;

	public List<LocalizedString> HelloPhrases;

	public List<LocalizedString> FinishDealPhrases;
}
