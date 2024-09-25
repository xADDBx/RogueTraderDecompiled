using System.Collections.Generic;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartPreventInterruption : BaseUnitPart, IHashable
{
	[JsonProperty]
	private readonly List<BlueprintActivatableAbility> m_NonInterruptiveAbilities = new List<BlueprintActivatableAbility>();

	public void AddNonInterruptive(BlueprintActivatableAbility ability)
	{
		m_NonInterruptiveAbilities.Add(ability);
	}

	public void RemoveNonInterruptive(BlueprintActivatableAbility ability)
	{
		m_NonInterruptiveAbilities.Remove(ability);
	}

	public bool CanInterrupt(BlueprintActivatableAbility ability)
	{
		return !m_NonInterruptiveAbilities.Contains(ability);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<BlueprintActivatableAbility> nonInterruptiveAbilities = m_NonInterruptiveAbilities;
		if (nonInterruptiveAbilities != null)
		{
			for (int i = 0; i < nonInterruptiveAbilities.Count; i++)
			{
				Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(nonInterruptiveAbilities[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
