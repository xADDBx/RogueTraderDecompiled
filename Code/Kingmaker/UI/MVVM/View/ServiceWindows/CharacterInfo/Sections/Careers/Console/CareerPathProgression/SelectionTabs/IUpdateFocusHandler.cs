using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;

public interface IUpdateFocusHandler : ISubscriber
{
	void HandleFocus();
}
