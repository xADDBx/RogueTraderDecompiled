using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilityPatternSettings : UnitPart, IHashable
{
	private readonly List<(EntityFactComponent Runtime, OverrideAbilityPatternSettings Component)> m_PatternEntries = new List<(EntityFactComponent, OverrideAbilityPatternSettings)>();

	private readonly List<(EntityFactComponent Runtime, OverrideAbilityPatternRadius Component)> m_OverrideRadius = new List<(EntityFactComponent, OverrideAbilityPatternRadius)>();

	public static IAbilityAoEPatternProvider GetAbilityPatternSettings(AbilityData ability, [CanBeNull] IAbilityAoEPatternProvider currentPattern = null)
	{
		IAbilityAoEPatternProvider abilityAoEPatternProvider = null;
		PartAbilityPatternSettings abilityPatternSettingsOptional = ability.Caster.GetAbilityPatternSettingsOptional();
		if (abilityPatternSettingsOptional != null)
		{
			foreach (var patternEntry in abilityPatternSettingsOptional.m_PatternEntries)
			{
				using (patternEntry.Runtime.RequestEventContext())
				{
					AbilityAoEPatternSettings patternSettings = patternEntry.Component.GetPatternSettings(ability);
					if (patternSettings != null)
					{
						abilityAoEPatternProvider = patternSettings;
						break;
					}
				}
			}
		}
		if (abilityAoEPatternProvider == null)
		{
			abilityAoEPatternProvider = ability.Blueprint.PatternSettings ?? currentPattern;
		}
		AoEPattern pattern = abilityAoEPatternProvider?.Pattern;
		if (abilityPatternSettingsOptional != null && abilityAoEPatternProvider != null)
		{
			foreach (var item in abilityPatternSettingsOptional.m_OverrideRadius)
			{
				using (item.Runtime.RequestEventContext())
				{
					pattern = item.Component.OverrideRadius(ability, abilityAoEPatternProvider.Pattern);
				}
			}
		}
		abilityAoEPatternProvider?.OverridePattern(pattern);
		return abilityAoEPatternProvider;
	}

	public void Add(OverrideAbilityPatternSettings component)
	{
		m_PatternEntries.Add((component.Runtime, component));
	}

	public void Remove(OverrideAbilityPatternSettings component)
	{
		m_PatternEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	public void Add(OverrideAbilityPatternRadius component)
	{
		m_OverrideRadius.Add((component.Runtime, component));
	}

	public void Remove(OverrideAbilityPatternRadius component)
	{
		m_OverrideRadius.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_PatternEntries.Empty() && m_OverrideRadius.Empty())
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
