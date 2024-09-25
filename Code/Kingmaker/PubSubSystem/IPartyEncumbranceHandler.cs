using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;

namespace Kingmaker.PubSubSystem;

public interface IPartyEncumbranceHandler : ISubscriber
{
	void ChangePartyEncumbrance(Encumbrance prevEncumbrance);
}
