using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface ISetInventorySorterHandler : ISubscriber
{
	void HandleSetInventorySorter(ItemsSorterType sorterType);
}
