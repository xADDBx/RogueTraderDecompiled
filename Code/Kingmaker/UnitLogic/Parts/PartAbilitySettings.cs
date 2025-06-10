using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilitySettings : BaseUnitPart, IInterruptTurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnEndHandler, IHashable
{
	[JsonProperty]
	public RestrictionCalculator InterruptionAbilityRestrictions;

	private readonly List<(EntityFactComponent Runtime, OverrideAbilityThreatenedAreaSetting Component)> m_ThreatenedAreaEntries = new List<(EntityFactComponent, OverrideAbilityThreatenedAreaSetting)>();

	public static BlueprintAbility.UsingInThreateningAreaType GetThreatenedAreaSetting(MechanicEntity caster, BlueprintAbility blueprint)
	{
		PartAbilitySettings abilitySettingsOptional = caster.GetAbilitySettingsOptional();
		if (abilitySettingsOptional == null)
		{
			return blueprint.UsingInThreateningArea;
		}
		foreach (var threatenedAreaEntry in abilitySettingsOptional.m_ThreatenedAreaEntries)
		{
			using (threatenedAreaEntry.Runtime.RequestEventContext())
			{
				BlueprintAbility.UsingInThreateningAreaType? threatenedAreaRule = threatenedAreaEntry.Component.GetThreatenedAreaRule();
				if (threatenedAreaRule.HasValue)
				{
					return threatenedAreaRule.GetValueOrDefault();
				}
			}
		}
		return blueprint.UsingInThreateningArea;
	}

	public void Add(OverrideAbilityThreatenedAreaSetting component)
	{
		m_ThreatenedAreaEntries.Add((component.Runtime, component));
	}

	public void Remove(OverrideAbilityThreatenedAreaSetting component)
	{
		m_ThreatenedAreaEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_ThreatenedAreaEntries.Empty() && InterruptionAbilityRestrictions == null)
		{
			RemoveSelf();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			InterruptionAbilityRestrictions = interruptionData.RestrictionsOnInterrupt;
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			InterruptionAbilityRestrictions = null;
			RemoveSelfIfEmpty();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<RestrictionCalculator>.GetHash128(InterruptionAbilityRestrictions);
		result.Append(ref val2);
		return result;
	}
}
