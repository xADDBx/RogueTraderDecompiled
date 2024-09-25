using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/DirectionBetweenPoints")]
[AllowMultipleComponents]
[TypeId("e0c8473c4e2748745a0c82e9bd32fbf8")]
public class DirectionBetweenPoints : PositionEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public PositionEvaluator From;

	[ValidateNotNull]
	[SerializeReference]
	public PositionEvaluator To;

	public override string GetCaption()
	{
		return $"Direction from {From} to {To}";
	}

	protected override Vector3 GetValueInternal()
	{
		if (!From.TryGetValue(out var value) || !To.TryGetValue(out var value2))
		{
			throw new FailToEvaluateException(this);
		}
		return (value2 - value).normalized;
	}
}
