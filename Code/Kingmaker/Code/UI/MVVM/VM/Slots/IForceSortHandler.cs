using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IForceSortHandler : ISubscriber
{
	void HandleForceSort();
}
