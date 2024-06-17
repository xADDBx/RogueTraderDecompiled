using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFakePsychicPhenomenaTrigger : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleFakePsychicPhenomena(bool isPsychicPhenomena, bool isPerilsOfTheWarp);
}
