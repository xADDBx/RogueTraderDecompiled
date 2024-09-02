using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("6c00fcc900e9e82499d86ef9e35ea70d")]
public class ContextConditionBuffRank : ContextCondition
{
	public BlueprintBuffReference Buff;

	public ContextValue RankValue;

	public bool BuffFromCaster;

	protected override string GetConditionCaption()
	{
		return $"Check if target {Buff} buff rank is higher or equal than {RankValue}";
	}

	protected override bool CheckCondition()
	{
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		EntityFact entityFact = base.Target.Entity.Facts.Get((BlueprintBuff)Buff);
		if (entityFact == null)
		{
			return false;
		}
		if (BuffFromCaster && entityFact.MaybeContext?.MaybeCaster != base.Context.MaybeCaster)
		{
			PFLog.Default.Error("Caster of the buff is not the caster of this ability");
			return false;
		}
		return entityFact.GetRank() >= RankValue.Calculate(base.Context);
	}
}
