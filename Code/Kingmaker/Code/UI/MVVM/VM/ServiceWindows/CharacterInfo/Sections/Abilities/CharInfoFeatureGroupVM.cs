using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureGroupVM : BaseFeatureGroupVM<CharInfoFeatureVM>
{
	public CharInfoFeatureGroupVM([NotNull] List<CharInfoFeatureVM> featuresListGroup, string label = null, string tooltipKey = null)
		: base(featuresListGroup, label, tooltipKey)
	{
	}
}
