using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/UnitMaxHPPercent")]
[AllowMultipleComponents]
[TypeId("068be78f66154fd4389398a70aa0274d")]
public class UnitMaxHPPercent : IntEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public int Percent;

	public override string GetCaption()
	{
		return $"{Percent}% of {Unit.GetCaption()} max HP";
	}

	protected override int GetValueInternal()
	{
		PartHealth healthOptional = Unit.GetValue().GetHealthOptional();
		if (healthOptional == null)
		{
			return 1;
		}
		return healthOptional.MaxHitPoints * Percent / 100;
	}
}
