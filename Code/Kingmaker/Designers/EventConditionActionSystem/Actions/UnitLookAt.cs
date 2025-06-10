using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
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
		AbstractUnitEntity value = Unit.GetValue();
		Vector3 value2 = Position.GetValue();
		if (value.View.IsVisible)
		{
			value.LookAt(value2);
		}
		else
		{
			value.ForceLookAt(value2);
		}
	}
}
