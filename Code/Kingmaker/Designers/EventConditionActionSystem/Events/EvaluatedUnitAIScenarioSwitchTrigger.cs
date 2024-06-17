using Kingmaker.AI.Scenarios;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("ec296cfbd1babc049b00457aefeea047")]
public class EvaluatedUnitAIScenarioSwitchTrigger : EntityFactComponentDelegate, IUnitAiScenarioHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	[HideIf("triggerOnAnyScenario")]
	private AiScenarioType Scenario;

	[SerializeField]
	private bool triggerOnAnyScenario;

	[SerializeField]
	private bool triggerOnDeactivate;

	[SerializeField]
	private ActionList Actions;

	public void HandleScenarioActivated(AiScenario scenario)
	{
		if (!triggerOnDeactivate)
		{
			CheckAndRunActions(EventInvokerExtensions.BaseUnitEntity, scenario);
		}
	}

	public void HandleScenarioDeactivated(AiScenario scenario)
	{
		if (triggerOnDeactivate)
		{
			CheckAndRunActions(EventInvokerExtensions.BaseUnitEntity, scenario);
		}
	}

	private void CheckAndRunActions(BaseUnitEntity unit, AiScenario scenario)
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Event {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else if (baseUnitEntity == unit && (triggerOnAnyScenario || (scenario is HoldPositionScenario && Scenario == AiScenarioType.HoldPosition) || (scenario is HoldPositionScenario && Scenario == AiScenarioType.Breach) || (scenario is HoldPositionScenario && Scenario == AiScenarioType.PriorityTarget) || (scenario is PriorityTargetScenario && Scenario == AiScenarioType.PriorityTarget)))
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
