using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.PubSubSystem;

public interface IInteractionRestrictionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleMissingInteractionSkill(MapObjectView mapObjectView, StatType skill);

	void HandleJammed(MapObjectView mapObjectView);

	void HandleCantDisarmTrap(TrapObjectView trap);
}
