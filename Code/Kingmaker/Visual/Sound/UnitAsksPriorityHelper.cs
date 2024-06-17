namespace Kingmaker.Visual.Sound;

public static class UnitAsksPriorityHelper
{
	private static BarkWrapper[] BarkWrapperPrioritizationGroups = new BarkWrapper[11];

	public static void RegisterBark(BarkWrapper wrapper)
	{
		if (wrapper != null)
		{
			int prioritizationGroup = wrapper.Bark.PrioritizationGroup;
			UnitAsksComponent.Bark bark = wrapper.Bark;
			if (BarkWrapperPrioritizationGroups[prioritizationGroup] != null && !BarkWrapperPrioritizationGroups[prioritizationGroup].IsPlaying)
			{
				RemoveCurrentHighestPriorityBark(prioritizationGroup);
			}
			if (BarkWrapperPrioritizationGroups[prioritizationGroup] == null)
			{
				BarkWrapperPrioritizationGroups[prioritizationGroup] = wrapper;
			}
			else if (wrapper != BarkWrapperPrioritizationGroups[prioritizationGroup] && bark.Priority < BarkWrapperPrioritizationGroups[prioritizationGroup].Bark.Priority)
			{
				BarkWrapperPrioritizationGroups[prioritizationGroup].UnitBarksManager.DiscardCurrentActiveBark();
				BarkWrapperPrioritizationGroups[prioritizationGroup] = wrapper;
			}
		}
	}

	private static void RemoveCurrentHighestPriorityBark(int prioritizationGroup)
	{
		BarkWrapperPrioritizationGroups[prioritizationGroup] = null;
	}

	public static BarkWrapper GetCurrentHighestPriorityBark(int prioritizationGroup)
	{
		return BarkWrapperPrioritizationGroups[prioritizationGroup];
	}
}
