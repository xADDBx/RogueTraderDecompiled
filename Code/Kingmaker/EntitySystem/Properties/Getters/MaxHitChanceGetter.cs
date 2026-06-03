using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("0d938b9eac854d0385b956795fe45f6b")]
public class MaxHitChanceGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional
{
	public PropertyTargetType Target;

	[InfoBox("Don't use ability in current context for calculating Max Hit Chance (base value will always be 100)")]
	public bool IgnoreAbility;

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is UnitEntity initiator))
		{
			return 100;
		}
		UnitEntity target = this.GetTargetByType(Target) as UnitEntity;
		AbilityData ability = ((!IgnoreAbility) ? this.GetAbility() : null);
		RuleCalculateHitChanceBorder ruleCalculateHitChanceBorder = new RuleCalculateHitChanceBorder(initiator, target, ability);
		Rulebook.Trigger(ruleCalculateHitChanceBorder);
		return ruleCalculateHitChanceBorder.Result;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Max Hit Chance of " + FormulaTargetScope.Current;
	}
}
