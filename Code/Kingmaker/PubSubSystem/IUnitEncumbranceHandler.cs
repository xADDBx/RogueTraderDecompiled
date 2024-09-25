using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;

namespace Kingmaker.PubSubSystem;

public interface IUnitEncumbranceHandler : ISubscriber
{
	void ChangeUnitEncumbrance(Encumbrance prevEncumbrance);
}
