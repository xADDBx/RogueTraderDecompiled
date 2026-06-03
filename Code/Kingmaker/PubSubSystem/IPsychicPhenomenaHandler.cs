using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.PubSubSystem;

public interface IPsychicPhenomenaHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandlePsychicPhenomena(RuleCalculatePsychicPhenomenaEffect rule);
}
