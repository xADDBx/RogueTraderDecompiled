using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d6d27a06a45444229b9fbd89a137093d")]
public class MovementPointMetaBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateMovementPoints>, IRulebookHandler<RuleCalculateMovementPoints>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue Value;

	public void OnEventAboutToTrigger(RuleCalculateMovementPoints evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateMovementPoints evt)
	{
		ModifiableValue modifiableValue = ((MechanicEntity)evt.Initiator).GetCombatStateOptional()?.WarhammerInitialAPBlue;
		int num = ((modifiableValue != null) ? ((int)modifiableValue) : 0);
		if (evt.Result - num > 0)
		{
			evt.Result += Value.Calculate(base.Context);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
