using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("88a988badcc84ea8847e21626bee82e5")]
public class PropertyCalculatorGetter : PropertyGetter
{
	public PropertyCalculator Value;

	public override bool AddBracketsAroundInnerCaption => false;

	protected override int GetBaseValue()
	{
		return Value.GetValue(base.PropertyContext);
	}

	protected override string GetInnerCaption()
	{
		return Value.ToString();
	}
}
