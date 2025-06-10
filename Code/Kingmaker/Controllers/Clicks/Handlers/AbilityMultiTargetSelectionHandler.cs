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

	private IAbilityMultiTarget m_AbilityMultiTarget;

	public IReadOnlyList<TargetWrapper> Targets => m_Targets;

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
		m_AbilityMultiTarget = abilityData?.Blueprint.GetComponent<IAbilityMultiTarget>();
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
		if (m_AbilityMultiTarget == null)
		{
			if (m_Targets.Count != 0)
			{
				return null;
			}
			return m_RootAbilityData;
		}
		if (!m_AbilityMultiTarget.TryGetNextTargetAbilityAndCaster(m_RootAbilityData, m_Targets.Count, out var ability, out var caster))
		{
			return null;
		}
		if (ability == m_RootAbilityData.Blueprint && caster == m_RootAbilityData.Caster)
		{
			return m_RootAbilityData;
		}
		return new AbilityData(ability ?? m_RootAbilityData.Blueprint, caster ?? m_RootAbilityData.Caster);
	}
}
