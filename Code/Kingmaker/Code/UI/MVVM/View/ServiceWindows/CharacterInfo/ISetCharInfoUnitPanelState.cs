using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public interface ISetCharInfoUnitPanelState : ISubscriber
{
	void SetUnitPanelNavigationState(bool state);
}
