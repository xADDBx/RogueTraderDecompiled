using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/RelativePosition")]
[AllowMultipleComponents]
[TypeId("d92fe0d9452d71b409f0a2ba65152165")]
public class PositionRelativeToUnit : PositionEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public float Distance;

	[Range(-180f, 180f)]
	public float Angle;

	protected override Vector3 GetValueInternal()
	{
		if (Unit == null)
		{
			return Vector3.zero;
		}
		AbstractUnitEntity value = Unit.GetValue();
		return value.Position + Quaternion.Euler(0f, value.Orientation + Angle, 0f) * Vector3.forward * Distance;
	}

	public override string GetCaption()
	{
		return string.Format("{0}m from {1}", Distance, (Unit == null) ? "-null-" : Unit.GetCaption());
	}
}
