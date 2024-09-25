using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("5490dbe723156cb459fa639153196db0")]
public class FactOwner : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		object obj = ContextData<FactData>.Current?.Fact?.IOwner as BaseUnitEntity;
		if (obj == null)
		{
			Buff.Data current = ContextData<Buff.Data>.Current;
			if (current == null)
			{
				return null;
			}
			Buff buff = current.Buff;
			if (buff == null)
			{
				return null;
			}
			obj = buff.Owner;
		}
		return (AbstractUnitEntity)obj;
	}

	public override string GetCaption()
	{
		return "Fact Owner";
	}
}
