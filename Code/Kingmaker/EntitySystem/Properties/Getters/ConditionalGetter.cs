using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("2b7102ebfff042409c7f1493b16125a9")]
public class ConditionalGetter : PropertyGetter
{
	public PropertyCalculator Condition;

	public PropertyCalculator True;

	public PropertyCalculator False;

	protected override int GetBaseValue()
	{
		if (!Condition.GetBoolValue(base.PropertyContext))
		{
			return False.GetValue(base.PropertyContext);
		}
		return True.GetValue(base.PropertyContext);
	}

	protected override string GetInnerCaption()
	{
		return $"{Condition} ? {True} : {False}";
	}
}
