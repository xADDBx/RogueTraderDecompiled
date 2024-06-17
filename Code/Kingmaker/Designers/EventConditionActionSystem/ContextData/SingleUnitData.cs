using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public abstract class SingleUnitData<TSelf> : AbstractUnitData<TSelf> where TSelf : ContextData<TSelf>, new()
{
	public new BaseUnitEntity Unit => (BaseUnitEntity)base.Unit;

	public SingleUnitData<TSelf> Setup([NotNull] BaseUnitEntity unit)
	{
		Setup((AbstractUnitEntity)unit);
		return this;
	}
}
