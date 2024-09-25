using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonizationEachStatHandler : ISubscriber
{
	void HandleContentmentChanged(Colony colony, int modifier);

	void HandleContentmentInAllColoniesChanged(int modifier);

	void HandleEfficiencyChanged(Colony colony, int modifier);

	void HandleEfficiencyInAllColoniesChanged(int modifier);

	void HandleSecurityChanged(Colony colony, int modifier);

	void HandleSecurityInAllColoniesChanged(int modifier);
}
