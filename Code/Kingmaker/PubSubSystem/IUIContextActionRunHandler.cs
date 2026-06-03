using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.PubSubSystem;

public interface IUIContextActionRunHandler : ISubscriber
{
	void HandleOnContextActionRun(MechanicsContext context, MechanicEntity caster, MechanicEntity target);
}
