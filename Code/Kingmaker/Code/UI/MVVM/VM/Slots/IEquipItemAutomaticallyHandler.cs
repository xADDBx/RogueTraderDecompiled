using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IEquipItemAutomaticallyHandler : ISubscriber
{
	void HandleEquipItemAutomatically(ItemEntity item);
}
