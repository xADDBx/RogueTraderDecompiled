using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/EqualTo")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("8c447b3f209747e5bf7d625988074009")]
public class EqualTo : Condition
{
	public bool FloatValues;

	[HideIf("FloatValues")]
	[SerializeReference]
	public IntEvaluator Value;

	[HideIf("FloatValues")]
	[SerializeReference]
	public IntEvaluator MinValue;

	[ShowIf("FloatValues")]
	[SerializeReference]
	public FloatEvaluator FloatValue;

	[ShowIf("FloatValues")]
	[SerializeReference]
	public FloatEvaluator FloatMinValue;

	protected override string GetConditionCaption()
	{
		return "Сравнивает два числа на равенство, например:\n(" + ((!FloatValues) ? Value?.ToString() : FloatValue?.ToString()) + ") == (" + ((!FloatValues) ? MinValue?.ToString() : FloatMinValue?.ToString()) + ")";
	}

	protected override bool CheckCondition()
	{
		if (FloatValues)
		{
			return Math.Abs(FloatValue.GetValue() - FloatMinValue.GetValue()) >= 1E-06f;
		}
		return Value.GetValue() == MinValue.GetValue();
	}
}
