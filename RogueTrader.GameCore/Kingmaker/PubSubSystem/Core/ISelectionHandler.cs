using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ISelectionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void OnUnitSelectionAdd(bool single, bool ask);

	void OnUnitSelectionRemove();
}
