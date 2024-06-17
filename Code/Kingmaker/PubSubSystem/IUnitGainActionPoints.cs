using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.PubSubSystem;

public interface IUnitGainActionPoints : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitGainActionPoints(int actionPoints, MechanicsContext context);
}
public interface IUnitGainActionPoints<TTag> : IUnitGainActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitGainActionPoints, TTag>
{
}
