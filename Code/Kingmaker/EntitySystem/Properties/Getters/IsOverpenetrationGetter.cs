using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("3a49afcdfc7a4aeea20f98ffcd06bbf2")]
public class IsOverpenetrationGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		RulePerformAttackRoll rulePerformAttackRoll = Rulebook.CurrentContext.LastEventOfType<RulePerformAttackRoll>();
		if (rulePerformAttackRoll == null || !rulePerformAttackRoll.IsOverpenetration)
		{
			RulePerformAttack rulePerformAttack = Rulebook.CurrentContext.LastEventOfType<RulePerformAttack>();
			if (rulePerformAttack == null || !rulePerformAttack.IsOverpenetration)
			{
				return 0;
			}
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check attack is overpenetration";
	}
}
