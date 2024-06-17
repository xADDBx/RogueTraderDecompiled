using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/UnitTransform")]
[AllowMultipleComponents]
[TypeId("93a8e117c084af945a2d44ce45cfa786")]
public class UnitTransform : TransformEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override Transform GetValueInternal()
	{
		return Unit.GetValue().View.transform;
	}

	public override string GetCaption()
	{
		return $"{Unit}";
	}
}
