using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f07b2f605695dc64d954bf1827049f86")]
public class UseActionPoints : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	private int AP;

	[SerializeField]
	private int MP;

	[SerializeField]
	private bool EndTurn;

	public override string GetCaption()
	{
		return string.Format("Use {0} action points ({1} AP, {2} MP){3}", Unit, AP, MP, EndTurn ? " and end turn" : "");
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
		}
		else if (baseUnitEntity.CombatState.IsInCombat)
		{
			baseUnitEntity.CombatState.SpendActionPoints(AP, MP);
			if (EndTurn && Game.Instance.TurnController.CurrentUnit == baseUnitEntity)
			{
				Game.Instance.TurnController.RequestEndTurn();
			}
		}
	}
}
