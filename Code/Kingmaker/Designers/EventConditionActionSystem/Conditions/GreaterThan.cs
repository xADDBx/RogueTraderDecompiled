using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("3b8a817c2d1aed24ca0779940550d2bc")]
public class GreaterThan : Condition
{
	public bool OrEqualTo;

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

	public override string GetDescription()
	{
		return "Сравнивает два числа. Можно проверять целочисленные и вещественные числа с помощью строгих (>) и нестрогих (>=) неравенств, например:\n(" + ((!FloatValues) ? Value?.ToString() : FloatValue?.ToString()) + ") > (" + ((!FloatValues) ? MinValue?.ToString() : FloatMinValue?.ToString()) + ")";
	}

	protected override string GetConditionCaption()
	{
		return ((!FloatValues) ? Value?.ToString() : FloatValue?.ToString()) + " greater than (or equal) " + ((!FloatValues) ? MinValue?.ToString() : FloatMinValue?.ToString());
	}

	protected override bool CheckCondition()
	{
		bool flag;
		bool flag2;
		if (FloatValues)
		{
			flag = FloatValue.GetValue() > FloatMinValue.GetValue();
			flag2 = Math.Abs(FloatValue.GetValue() - FloatMinValue.GetValue()) >= 1E-06f;
		}
		else
		{
			flag = Value.GetValue() > MinValue.GetValue();
			flag2 = Value.GetValue() == MinValue.GetValue();
		}
		if (!OrEqualTo)
		{
			return flag;
		}
		return flag || flag2;
	}
}
