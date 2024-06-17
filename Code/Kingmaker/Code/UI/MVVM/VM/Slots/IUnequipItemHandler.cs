using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IUnequipItemHandler : ISubscriber
{
	void HandleUnequipItem();
}
