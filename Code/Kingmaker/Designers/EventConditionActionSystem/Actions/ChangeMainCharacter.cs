using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("3e87f8eb96e94fb8ade22f6aa8b57ecf")]
public class ChangeMainCharacter : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetCaption()
	{
		return "Change main character";
	}

	protected override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity companionToMainCharacter))
		{
			_ = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
		}
		else
		{
			Game.Instance.Player.SetCompanionToMainCharacter(companionToMainCharacter);
		}
	}
}
