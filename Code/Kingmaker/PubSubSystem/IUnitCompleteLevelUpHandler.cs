using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitCompleteLevelUpHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitCompleteLevelup();
}
