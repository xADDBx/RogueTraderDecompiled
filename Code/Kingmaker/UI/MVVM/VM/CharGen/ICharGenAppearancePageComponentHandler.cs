using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenAppearancePageComponentHandler : ISubscriber
{
	void HandleComponentChanged(CharGenAppearancePageComponent pageComponent);
}
