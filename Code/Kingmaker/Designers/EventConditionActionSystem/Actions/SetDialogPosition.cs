using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("445035f0eea06f242874dfd1757d9a00")]
public class SetDialogPosition : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public PositionEvaluator Position;

	public override string GetCaption()
	{
		return $"Set Dialog Position ({Position})";
	}

	protected override void RunAction()
	{
		Game.Instance.DialogController.DialogPosition = Position.GetValue();
	}
}
