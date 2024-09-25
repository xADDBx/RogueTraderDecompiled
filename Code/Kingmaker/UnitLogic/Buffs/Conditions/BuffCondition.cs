using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.UnitLogic.Buffs.Conditions;

[TypeId("cff81f02c313ef244b5a1b79c9068e83")]
public abstract class BuffCondition : Condition
{
	protected Buff Buff => ContextData<Buff.Data>.Current?.Buff;
}
