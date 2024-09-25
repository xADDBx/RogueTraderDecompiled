using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

public interface IPickLockHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandlePickLockSuccess(MapObjectView mapObjectView);

	void HandlePickLockFail(MapObjectView mapObjectView, bool critical);
}
