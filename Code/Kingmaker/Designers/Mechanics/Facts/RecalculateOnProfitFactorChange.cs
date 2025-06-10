using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("b527cf50bdeb41f1bb6ae5f750f55780")]
public class RecalculateOnProfitFactorChange : MechanicEntityFactComponentDelegate, IProfitFactorHandler, ISubscriber, IHashable
{
	public void HandleProfitFactorModifierAdded(float max, ProfitFactorModifier modifier)
	{
		base.Fact.Reapply();
	}

	public void HandleProfitFactorModifierRemoved(float max, ProfitFactorModifier modifier)
	{
		base.Fact.Reapply();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
