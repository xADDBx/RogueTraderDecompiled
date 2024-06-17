using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public abstract class AbstractUnitData<TSelf> : ContextData<TSelf> where TSelf : ContextData<TSelf>, new()
{
	private AbstractUnitEntity m_Unit;

	public AbstractUnitEntity Unit => m_Unit;

	public AbstractUnitData<TSelf> Setup([NotNull] AbstractUnitEntity unit)
	{
		m_Unit = unit;
		return this;
	}

	protected override void Reset()
	{
		m_Unit = null;
	}
}
