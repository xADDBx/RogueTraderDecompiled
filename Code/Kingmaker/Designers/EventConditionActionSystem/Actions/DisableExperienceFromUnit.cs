using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("9b554d5fe6a9f3643aff4c8925dd7ab5")]
public class DisableExperienceFromUnit : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return $"Выключает выдачу опыта за смерть юнита {Unit}.";
	}

	public override string GetCaption()
	{
		return $"Disable experience from {Unit}";
	}

	protected override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			baseUnitEntity.GiveExperienceOnDeath = false;
		}
	}
}
