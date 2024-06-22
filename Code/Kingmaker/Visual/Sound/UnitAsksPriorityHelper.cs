namespace Kingmaker.Visual.Sound;

public static class UnitAsksPriorityHelper
{
	private static BarkWrapper[] m_BarkWrapperPrioritizationGroups = new BarkWrapper[11];

	public static void RegisterBark(BarkWrapper wrapper)
	{
		if (wrapper != null)
		{
			int prioritizationGroup = wrapper.Bark.PrioritizationGroup;
			UnitAsksComponent.Bark bark = wrapper.Bark;
			if (m_BarkWrapperPrioritizationGroups[prioritizationGroup] != null && !m_BarkWrapperPrioritizationGroups[prioritizationGroup].IsPlaying)
			{
				RemoveCurrentHighestPriorityBark(prioritizationGroup);
			}
			if (m_BarkWrapperPrioritizationGroups[prioritizationGroup] == null)
			{
				m_BarkWrapperPrioritizationGroups[prioritizationGroup] = wrapper;
			}
			else if (wrapper != m_BarkWrapperPrioritizationGroups[prioritizationGroup] && bark.Priority < m_BarkWrapperPrioritizationGroups[prioritizationGroup].Bark.Priority)
			{
				m_BarkWrapperPrioritizationGroups[prioritizationGroup].UnitBarksManager.DiscardCurrentActiveBark();
				m_BarkWrapperPrioritizationGroups[prioritizationGroup] = wrapper;
			}
		}
	}

	private static void RemoveCurrentHighestPriorityBark(int prioritizationGroup)
	{
		m_BarkWrapperPrioritizationGroups[prioritizationGroup] = null;
	}

	public static BarkWrapper GetCurrentHighestPriorityBark(int prioritizationGroup)
	{
		return m_BarkWrapperPrioritizationGroups[prioritizationGroup];
	}
}
