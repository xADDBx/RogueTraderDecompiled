using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICommandServiceWindowUIHandler : ISubscriber
{
	void HandleOpenWindowOfType(ServiceWindowsType type);
}
