using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitForceMoveCollisionHandler : ISubscriber
{
	void HandleUnitCollisionFromForceMove(MechanicEntity pushed, MechanicEntity pusher, int damage);
}
