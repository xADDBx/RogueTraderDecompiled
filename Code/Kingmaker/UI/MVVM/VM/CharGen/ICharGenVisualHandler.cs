using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenVisualHandler : ISubscriber
{
	void HandleShowCloth(bool showCloth);
}
