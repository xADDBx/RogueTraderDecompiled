using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/UnitCurrentHPPercent")]
[AllowMultipleComponents]
[TypeId("f49cde37db540ab469caedc3ad9ac5ec")]
public class UnitCurrentHPPercent : IntEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override int GetValueInternal()
	{
		PartHealth healthOptional = Unit.GetValue().GetHealthOptional();
		if (healthOptional == null)
		{
			return 100;
		}
		return (int)Math.Floor((float)healthOptional.HitPointsLeft * 100f / (float)healthOptional.MaxHitPoints);
	}

	public override string GetCaption()
	{
		return "Unit current HP percent";
	}
}
