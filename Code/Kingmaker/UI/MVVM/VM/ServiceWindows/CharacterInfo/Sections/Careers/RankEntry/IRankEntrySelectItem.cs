using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public interface IRankEntrySelectItem : IHasTooltipTemplates
{
	FeatureGroup? GetFeatureGroup();

	bool CanSelect();

	void UpdateFeatures();
}
