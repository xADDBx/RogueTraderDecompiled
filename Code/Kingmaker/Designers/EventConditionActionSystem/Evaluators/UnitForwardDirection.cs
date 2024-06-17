using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/UnitForwardDirection")]
[AllowMultipleComponents]
[TypeId("7fdc6b7ccebcfa94d85822075fe19e79")]
public class UnitForwardDirection : PositionEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetCaption()
	{
		return $"{Unit}'s forward";
	}

	protected override Vector3 GetValueInternal()
	{
		if (!Unit.TryGetValue(out var value) || value == null)
		{
			throw new FailToEvaluateException(this);
		}
		return value.Forward;
	}
}
