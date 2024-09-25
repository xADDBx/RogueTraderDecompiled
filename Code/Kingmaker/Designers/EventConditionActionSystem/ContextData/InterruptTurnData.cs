using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class InterruptTurnData : ContextData<InterruptTurnData>
{
	public MechanicEntity Unit;

	public MechanicEntity Source;

	public InterruptTurnData Setup(MechanicEntity unit, MechanicEntity source)
	{
		Unit = unit;
		Source = source;
		return this;
	}

	protected override void Reset()
	{
		Unit = null;
		Source = null;
	}
}
