using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public interface IRankEntryFocusHandler : ISubscriber
{
	void SetFocusOn(BaseRankEntryFeatureVM featureVM);
}
