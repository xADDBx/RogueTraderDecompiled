using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class AbilityMultiTargetSelectionHandler
{
	private readonly List<TargetWrapper> m_Targets = new List<TargetWrapper>();

	private AbilityData m_RootAbilityData;

	public IReadOnlyList<TargetWrapper> Targets => m_Targets;

	public bool IsMultiTargetSelected => AbilityMultiTarget != null;

	public IAbilityMultiTarget AbilityMultiTarget { get; private set; }

	public TargetWrapper GetLastTarget()
	{
		List<TargetWrapper> targets = m_Targets;
		if (targets == null || targets.Count <= 0)
		{
			return null;
		}
		List<TargetWrapper> targets2 = m_Targets;
		return targets2[targets2.Count - 1];
	}

	public TargetWrapper GetTargetByIndex(int targetIndex)
	{
		if (m_Targets == null || targetIndex < 0 || targetIndex >= m_Targets.Count)
		{
			return null;
		}
		return m_Targets[targetIndex];
	}

	public void OnRootAbilitySelected(AbilityData abilityData)
	{
		m_RootAbilityData = abilityData;
		AbilityMultiTarget = abilityData?.Blueprint.GetComponent<IAbilityMultiTarget>();
		if (abilityData == null)
		{
			m_Targets.Clear();
		}
	}

	public AbilityData AddTarget(TargetWrapper targetWrapper)
	{
		m_Targets.Add(targetWrapper);
		return GetAbilityForNextTarget();
	}

	public AbilityData GetAbilityForNextTarget()
	{
		if (AbilityMultiTarget == null)
		{
			if (m_Targets.Count != 0)
			{
				return null;
			}
			return m_RootAbilityData;
		}
		if (!AbilityMultiTarget.TryGetNextTargetAbility(m_RootAbilityData, m_Targets.Count, out var ability))
		{
			return null;
		}
		return ability;
	}
}
