using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUIVisibilityHandler : ISubscriber
{
	void HandleUIVisibilityChange(UIVisibilityFlags flags);
}
