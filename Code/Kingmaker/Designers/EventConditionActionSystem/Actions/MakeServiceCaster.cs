using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f46ee5f51ede46ccbec11dd944486a9b")]
public class MakeServiceCaster : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return $"Делает юнита {Unit} кастером для чтения свитков в хабе";
	}

	public override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity unit))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			Game.Instance.State.LoadedAreaState.ServiceCaster = unit.FromBaseUnitEntity();
		}
	}

	public override string GetCaption()
	{
		return "Make " + Unit?.ToString() + " service caster";
	}
}
