using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IIdentifyHandler : ISubscriber<IItemEntity>, ISubscriber
{
	void OnItemIdentified(BaseUnitEntity character);

	void OnFailedToIdentify();
}
