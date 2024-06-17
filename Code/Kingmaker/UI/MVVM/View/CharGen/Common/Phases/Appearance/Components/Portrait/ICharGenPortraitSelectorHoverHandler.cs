using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public interface ICharGenPortraitSelectorHoverHandler : ISubscriber
{
	void HandleHoverStart(PortraitData portrait);

	void HandleHoverStop();
}
