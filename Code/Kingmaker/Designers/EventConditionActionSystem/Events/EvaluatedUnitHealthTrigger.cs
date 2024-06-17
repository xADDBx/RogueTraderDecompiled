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

[ComponentName("Events/UnitHealthTrigger")]
[AllowMultipleComponents]
[TypeId("5d57d20f5d6e2c64688f23636662ad03")]
public class EvaluatedUnitHealthTrigger : EntityFactComponentDelegate, IDamageHandler, ISubscriber, IHashable
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public int Percentage;

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
		else if (baseUnitEntity == dealDamage.Target)
		{
			int num = (int)(0.01 * (double)Percentage * (double)baseUnitEntity.Health.MaxHitPoints);
			int hitPointsLeft = baseUnitEntity.Health.HitPointsLeft;
			bool flag = hitPointsLeft + dealDamage.Result > num;
			bool flag2 = hitPointsLeft <= num;
			if (flag && flag2)
			{
				Actions.Run();
			}
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
