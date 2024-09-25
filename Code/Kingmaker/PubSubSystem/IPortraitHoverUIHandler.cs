using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPortraitHoverUIHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandlePortraitHover(bool hover);
}
