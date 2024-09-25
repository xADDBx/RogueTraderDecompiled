using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class TargetsInPatternData : ContextData<TargetsInPatternData>
{
	public int TargetsInPattern;

	public TargetsInPatternData Setup(int targetsInPattern)
	{
		TargetsInPattern = targetsInPattern;
		return this;
	}

	protected override void Reset()
	{
		TargetsInPattern = 0;
	}
}
