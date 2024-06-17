using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenCloseHandler : ISubscriber
{
	void HandleClose(bool withComplete);
}
