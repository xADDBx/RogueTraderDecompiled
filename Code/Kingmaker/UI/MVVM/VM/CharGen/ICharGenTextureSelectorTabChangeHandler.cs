using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenTextureSelectorTabChangeHandler : ISubscriber
{
	void HandleTextureSelectorTabChange(CharGenAppearancePageComponent type, int tabIndex);
}
