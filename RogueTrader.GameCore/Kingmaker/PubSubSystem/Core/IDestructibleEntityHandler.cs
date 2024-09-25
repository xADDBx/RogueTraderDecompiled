using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IDestructibleEntityHandler : ISubscriber<IMapObjectEntity>, ISubscriber
{
	void HandleDestructionStageChanged(DestructionStage stage);
}
