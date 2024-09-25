using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class SavableTriggerData : ContextData<SavableTriggerData>
{
	public int ExecutesCount;

	public SavableTriggerData Setup(int executesCount)
	{
		ExecutesCount = executesCount;
		return this;
	}

	protected override void Reset()
	{
		ExecutesCount = 0;
	}
}
