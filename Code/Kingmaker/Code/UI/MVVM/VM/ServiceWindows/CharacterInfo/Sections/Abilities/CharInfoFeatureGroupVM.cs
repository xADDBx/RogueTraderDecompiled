using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureGroupVM : BaseFeatureGroupVM<CharInfoFeatureVM>
{
	public enum FeatureGroupType
	{
		Unknown,
		Abilities,
		Talents
	}

	public readonly FeatureGroupType GroupType;

	public CharInfoFeatureGroupVM([NotNull] List<CharInfoFeatureVM> featuresListGroup, string label = null, FeatureGroupType groupType = FeatureGroupType.Unknown, string tooltipKey = null)
		: base(featuresListGroup, label, tooltipKey)
	{
		GroupType = groupType;
	}
}
