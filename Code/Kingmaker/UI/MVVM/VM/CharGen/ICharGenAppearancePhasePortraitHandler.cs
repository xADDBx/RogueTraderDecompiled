using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenAppearancePhasePortraitHandler : ISubscriber
{
	void HandlePortraitTabChange(CharGenPortraitTab tab);
}
