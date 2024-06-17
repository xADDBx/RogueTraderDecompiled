using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/UnitPosition")]
[AllowMultipleComponents]
[TypeId("ac7da90e77cb5af44b778bfce17bb4aa")]
public class UnitPosition : PositionEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override Vector3 GetValueInternal()
	{
		if (!Unit.TryGetValue(out var value) || value == null)
		{
			throw new FailToEvaluateException(this);
		}
		return value.Position;
	}

	public override string GetCaption()
	{
		return $"{Unit}";
	}
}
