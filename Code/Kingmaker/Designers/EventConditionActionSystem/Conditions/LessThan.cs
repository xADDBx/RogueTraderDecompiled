using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/LessThan")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("4874411993c669b48b6ccb90d57a750b")]
public class LessThan : Condition
{
	public bool OrEqualTo;

	public bool FloatValues;

	[HideIf("FloatValues")]
	[SerializeReference]
	public IntEvaluator Value;

	[HideIf("FloatValues")]
	[SerializeReference]
	public IntEvaluator MaxValue;

	[ShowIf("FloatValues")]
	[SerializeReference]
	public FloatEvaluator FloatValue;

	[ShowIf("FloatValues")]
	[SerializeReference]
	public FloatEvaluator FloatMaxValue;

	public override string GetDescription()
	{
		return "Сравнивает два числа. Можно проверять целочисленные и вещественные числа с помощью строгих (<) и нестрогих (<=) неравенств, например:\n(" + ((!FloatValues) ? Value?.ToString() : FloatValue?.ToString()) + ") < (" + ((!FloatValues) ? MaxValue?.ToString() : FloatMaxValue?.ToString()) + ")";
	}

	protected override string GetConditionCaption()
	{
		return ((!FloatValues) ? Value?.ToString() : FloatValue?.ToString()) + " less than (or equal) " + ((!FloatValues) ? MaxValue?.ToString() : FloatMaxValue?.ToString());
	}

	protected override bool CheckCondition()
	{
		bool flag;
		bool flag2;
		if (FloatValues)
		{
			flag = FloatValue.GetValue() < FloatMaxValue.GetValue();
			flag2 = Math.Abs(FloatValue.GetValue() - FloatMaxValue.GetValue()) >= 1E-06f;
		}
		else
		{
			flag = Value.GetValue() < MaxValue.GetValue();
			flag2 = Value.GetValue() == MaxValue.GetValue();
		}
		if (!OrEqualTo)
		{
			return flag;
		}
		return flag || flag2;
	}
}
