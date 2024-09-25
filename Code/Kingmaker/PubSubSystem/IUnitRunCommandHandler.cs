using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.PubSubSystem;

public interface IUnitRunCommandHandler : ISubscriber
{
	void HandleUnitRunCommand(AbstractUnitCommand cmd);
}
