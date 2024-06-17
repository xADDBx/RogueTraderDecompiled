using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.PubSubSystem;

public interface IRunVirtualMoveCommandHandler : ISubscriber
{
	void HandleRunVirtualMoveCommand(AbstractUnitCommand command, UnitReference unit);
}
