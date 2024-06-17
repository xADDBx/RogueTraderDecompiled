using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[TypeId("01b3e135668a401418d96781fefaa83a")]
public abstract class AbilityCustomLogic : AbilityDeliverEffect
{
	public abstract void Cleanup(AbilityExecutionContext context);
}
