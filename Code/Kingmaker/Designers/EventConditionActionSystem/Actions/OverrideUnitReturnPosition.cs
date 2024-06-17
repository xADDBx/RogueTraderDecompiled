using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("a57fc378510834342bc162f176614737")]
public class OverrideUnitReturnPosition : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeReference]
	public PositionEvaluator Position;

	[SerializeReference]
	public FloatEvaluator Orientation;

	public override string GetCaption()
	{
		return $"Override {Unit} position to {Position}";
	}

	public override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		if (Position != null)
		{
			baseUnitEntity.CombatState.ReturnPosition = Position.GetValue();
		}
		if (Orientation != null)
		{
			baseUnitEntity.CombatState.ReturnOrientation = Orientation.GetValue();
		}
	}
}
