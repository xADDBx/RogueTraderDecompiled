using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitClickUIHandler : ISubscriber
{
	void HandleUnitRightClick([NotNull] AbstractUnitEntity entity);

	void HandleUnitConsoleInvoke([NotNull] AbstractUnitEntity entity);
}
