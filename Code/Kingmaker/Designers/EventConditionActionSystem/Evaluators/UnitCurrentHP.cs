using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/UnitCurrentHP")]
[AllowMultipleComponents]
[TypeId("6d36de660d10e7e4387bd84c1435daaa")]
public class UnitCurrentHP : IntEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override int GetValueInternal()
	{
		return Unit.GetValue().GetHealthOptional()?.HitPointsLeft ?? 1;
	}

	public override string GetCaption()
	{
		return "Unit current HP";
	}
}
