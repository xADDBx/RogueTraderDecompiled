using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("ea8e247dab444a78994efcf8bfdd6f2b")]
public class AbilityTargetsInPatternGetter : PropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IRule
{
	public bool CheckPerformAbilityRule;

	protected override int GetBaseValue()
	{
		int num = ContextData<TargetsInPatternData>.Current?.TargetsInPattern ?? 0;
		if (num > 0)
		{
			return num;
		}
		if (num == 0 && CheckPerformAbilityRule)
		{
			if (!(this.GetRule() is RulePerformAbility rulePerformAbility))
			{
				return num;
			}
			NodeList nodes = rulePerformAbility.Context.Pattern.Nodes;
			return num + nodes.Count((CustomGridNodeBase p) => (!(p.GetUnit()?.IsDead)) ?? false);
		}
		foreach (CustomGridNodeBase node in (this.GetMechanicContext().SourceAbilityContext ?? throw new Exception("Can't count targets in pattern of not casted ability")).Pattern.Nodes)
		{
			if (node.GetUnit() != null)
			{
				num++;
			}
		}
		return num;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of Targets in Pattern";
	}
}
