using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/UnitHealthByValueTrigger")]
[AllowMultipleComponents]
[TypeId("6e3e9f799661466f935e8408d09975a0")]
public class EvaluatedUnitHealthByValueTrigger : EntityFactComponentDelegate, IDamageHandler, ISubscriber, IHashable
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public int HpValue;

	public ActionList Actions;

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Event {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else if (baseUnitEntity == dealDamage.Target && baseUnitEntity.Health.HitPointsLeft <= HpValue)
		{
			Actions.Run();
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
