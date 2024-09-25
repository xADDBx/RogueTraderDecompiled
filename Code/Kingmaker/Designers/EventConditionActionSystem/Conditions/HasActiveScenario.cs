using Kingmaker.AI.Scenarios;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("f79e241de3fd966459da08c88babc0d7")]
public class HasActiveScenario : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	[HideIf("AnyScenario")]
	private AiScenarioType Scenario;

	[SerializeField]
	private bool AnyScenario;

	protected override string GetConditionCaption()
	{
		return string.Format("{0} has {1} scenario", Unit, AnyScenario ? "any" : ((object)Scenario));
	}

	protected override bool CheckCondition()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return false;
		}
		AiScenario currentScenario = baseUnitEntity.Brain.CurrentScenario;
		if (currentScenario == null)
		{
			return false;
		}
		if (!AnyScenario && (!(currentScenario is HoldPositionScenario) || Scenario != 0) && (!(currentScenario is HoldPositionScenario) || Scenario != AiScenarioType.Breach) && (!(currentScenario is HoldPositionScenario) || Scenario != AiScenarioType.PriorityTarget))
		{
			if (currentScenario is PriorityTargetScenario)
			{
				return Scenario == AiScenarioType.PriorityTarget;
			}
			return false;
		}
		return true;
	}
}
