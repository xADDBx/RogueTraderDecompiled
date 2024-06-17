using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.AI.Blueprints.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("e1a57ff30704c4b43bab7be3845248de")]
public class AiEscapeFromThreat : BlueprintComponent
{
	public EscapeType Type;
}
