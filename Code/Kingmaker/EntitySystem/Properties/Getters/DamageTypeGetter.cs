using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5acb988246cb416eaf70c152840975b3")]
public class DamageTypeGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public DamageType DamageType;

	protected override int GetBaseValue()
	{
		if ((this.GetRule() as IDamageHolderRule)?.Damage.Type != DamageType)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check Damage type is {DamageType}";
	}
}
