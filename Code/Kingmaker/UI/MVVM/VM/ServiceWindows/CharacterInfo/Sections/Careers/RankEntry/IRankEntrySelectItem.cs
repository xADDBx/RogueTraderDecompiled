using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public interface IRankEntrySelectItem : IHasTooltipTemplates
{
	int EntryRank { get; }

	BoolReactiveProperty HasUnavailableFeatures { get; }

	FeatureGroup? GetFeatureGroup();

	bool CanSelect();

	void UpdateFeatures();

	void UpdateReadOnlyState();

	void ToggleShowUnavailableFeatures();

	bool ContainsFeature(string key);
}
