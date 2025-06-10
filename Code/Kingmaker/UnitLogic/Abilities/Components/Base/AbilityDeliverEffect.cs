using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("d399f754312e9234bac88e25decd153a")]
public abstract class AbilityDeliverEffect : BlueprintComponent
{
	public virtual bool IsEngageUnit => false;

	public virtual bool IsMoveUnit => false;

	public abstract IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target);
}
