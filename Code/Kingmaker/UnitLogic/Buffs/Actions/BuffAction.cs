using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.UnitLogic.Buffs.Actions;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("61db6c10a2573084fbc97f33006da3b2")]
public abstract class BuffAction : ContextAction
{
	protected Buff Buff => ContextData<Buff.Data>.Current?.Buff;
}
