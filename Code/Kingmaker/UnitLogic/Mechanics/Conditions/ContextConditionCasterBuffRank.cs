using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("8509fb4e9245fa04ea730c4b5c6ffa98")]
public class ContextConditionCasterBuffRank : ContextCondition
{
	public BlueprintBuffReference Buff;

	public ContextValue RankValue;

	protected override string GetConditionCaption()
	{
		return $"Check if caster {Buff?.Get()} buff rank is higher or equal than {RankValue}";
	}

	protected override bool CheckCondition()
	{
		if (base.Context.MaybeCaster == null)
		{
			PFLog.Default.Error("Caster unit is missing");
			return false;
		}
		EntityFact entityFact = base.Context.MaybeCaster.Facts.Get((BlueprintBuff)Buff);
		if (entityFact == null)
		{
			return false;
		}
		return entityFact.GetRank() >= RankValue.Calculate(base.Context);
	}
}
