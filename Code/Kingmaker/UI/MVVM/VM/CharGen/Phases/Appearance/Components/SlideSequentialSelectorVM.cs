using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components;

public class SlideSequentialSelectorVM : StringSequentialSelectorVM
{
	public SlideSequentialSelectorVM(bool cyclical = true)
		: base(cyclical)
	{
	}

	public SlideSequentialSelectorVM(List<StringSequentialEntity> valueList, StringSequentialEntity current = null, bool cyclical = true)
		: base(valueList, current, cyclical)
	{
	}
}
