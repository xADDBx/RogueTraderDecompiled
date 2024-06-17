using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using TurnBased.Utility;

namespace Kingmaker.Controllers.Units;

public class UnitsProximityController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		using (TurnBasedZeroDeltaTime.Create())
		{
			UnitPartInteractions optional = unit.GetOptional<UnitPartInteractions>();
			if (optional == null)
			{
				return;
			}
			optional.UpdateCooldowns();
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if (partyAndPet.Stealth.Active)
				{
					continue;
				}
				float num = partyAndPet.DistanceTo(unit);
				if (!optional.Distances.TryGetValue(partyAndPet, out var value))
				{
					value = 1000000f;
				}
				optional.Distances[partyAndPet] = num;
				for (int i = 0; i < optional.Interactions.Count; i++)
				{
					IUnitInteraction unitInteraction = optional.Interactions[i];
					if (unitInteraction.IsApproach && !(optional.Cooldowns[i] > 0f) && !(num > (float)unitInteraction.Distance) && !(value < (float)unitInteraction.Distance) && unitInteraction.IsAvailable(partyAndPet, unit))
					{
						unit.Commands.InterruptAllInterruptible();
						unitInteraction.Interact(partyAndPet, unit);
						optional.Cooldowns[i] = unitInteraction.ApproachCooldown;
					}
				}
			}
		}
	}

	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		if (unit.IsInCombat)
		{
			return false;
		}
		return unit.GetOptional<UnitPartInteractions>()?.HasApproachInteractions ?? false;
	}
}
