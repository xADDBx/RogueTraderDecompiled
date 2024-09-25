using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Interaction;

public interface IUnitInteraction
{
	int Distance { get; }

	bool IsApproach { get; }

	float ApproachCooldown { get; }

	bool MainPlayerPreferred { get; }

	bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target);

	AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
