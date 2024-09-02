using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;

namespace Kingmaker.PubSubSystem;

public interface IUnitMoveToProperHandler : ISubscriber
{
	void HandleUnitMoveToProper(UnitMoveToProper cmd);
}
