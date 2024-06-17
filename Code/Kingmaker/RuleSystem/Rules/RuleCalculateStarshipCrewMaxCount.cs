using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateStarshipCrewMaxCount : RulebookEvent
{
	public ShipModuleType ShipModuleType { get; }

	public int Result { get; private set; }

	public int Default { get; }

	public int Bonus { get; set; }

	public RuleCalculateStarshipCrewMaxCount([NotNull] MechanicEntity initiator, ShipModuleSettings settings)
		: base(initiator)
	{
		Default = settings.CrewCount;
		ShipModuleType = settings.ModuleType;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Result = Default + Bonus;
	}
}
