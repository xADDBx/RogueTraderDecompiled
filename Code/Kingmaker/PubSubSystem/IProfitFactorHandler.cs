using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IProfitFactorHandler : ISubscriber
{
	void HandleProfitFactorModifierAdded(float max, ProfitFactorModifier modifier);

	void HandleProfitFactorModifierRemoved(float max, ProfitFactorModifier modifier);
}
