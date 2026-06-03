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

	[SerializeReference]
	public FloatEvaluator RotationOffset;

	public override string GetCaption()
	{
		return Unit?.ToString() + " look at " + Position;
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = Unit.GetValue();
		Vector3 value2 = Position.GetValue();
		value.DesiredOrientation = value.GetLookAtAngle(value2) + (RotationOffset?.GetValue() ?? 0f);
		if (!value.View.IsVisible)
		{
			value.ForceRotateToDesired();
		}
	}
}
