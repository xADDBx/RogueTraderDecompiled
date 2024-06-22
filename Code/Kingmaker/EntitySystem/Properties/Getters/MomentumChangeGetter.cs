using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5292e596037f7564482f878233e6e3f9")]
public class MomentumChangeGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		if (this.GetRule() is RulePerformMomentumChange rulePerformMomentumChange)
		{
			return rulePerformMomentumChange.ResultDeltaValue;
		}
		throw new ElementLogicException(this);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Momentum change";
	}
}
