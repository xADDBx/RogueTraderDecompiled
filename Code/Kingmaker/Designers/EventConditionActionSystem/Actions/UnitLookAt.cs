using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5842a0dd9c0748f43b45ba496edf8c03")]
public class UnitLookAt : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[ValidateNotNull]
	[SerializeReference]
	public PositionEvaluator Position;

	public override string GetCaption()
	{
		return Unit?.ToString() + " look at " + Position;
	}

	protected override void RunAction()
	{
		Unit.GetValue().LookAt(Position.GetValue());
	}
}
