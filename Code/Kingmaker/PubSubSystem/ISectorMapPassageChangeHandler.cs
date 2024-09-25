using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISectorMapPassageChangeHandler : ISubscriber<ISectorMapPassageEntity>, ISubscriber
{
	void HandleNewPassageCreated();

	void HandlePassageLowerDifficulty();
}
