using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class WarhammerUnitPartConcentrationController : BaseUnitPart, IAreaHandler, ISubscriber, IInitiatorRulebookHandler<RuleCalculateActionPoints>, IRulebookHandler<RuleCalculateActionPoints>, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateMovementPoints>, IRulebookHandler<RuleCalculateMovementPoints>, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IUnitBuffHandler, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, IEventTag<IUnitLifeStateChanged, EntitySubscriber>, IHashable
{
	[JsonProperty]
	public List<ConcentrationEntry> ConcentrationEntries = new List<ConcentrationEntry>();

	public int Index;

	public int AdditionalConcentrationActionPoints;

	public void NewEntry(Buff newBuff, BlueprintAbility ability)
	{
		BaseUnitEntity target = newBuff.Owner;
		ConcentrationEntry concentrationEntry = ConcentrationEntries.FirstOrDefault((ConcentrationEntry p) => p.Ability == ability && p.Target == target);
		if (concentrationEntry != null)
		{
			concentrationEntry.Buffs.Add(newBuff);
			return;
		}
		ConcentrationEntry concentrationEntry2 = new ConcentrationEntry(ability, target);
		concentrationEntry2.Buffs.Add(newBuff);
		concentrationEntry2.Index = Index;
		ConcentrationEntries.Add(concentrationEntry2);
		Index++;
		if (ConcentrationEntries.Sum((ConcentrationEntry p) => p.ActionPointCost) >= base.Owner.Attributes.WarhammerIntelligence.WarhammerBonus)
		{
			ConcentrationEntry concentrationEntry3 = ConcentrationEntries.MinBy((ConcentrationEntry p) => p.Index);
			concentrationEntry3.RemoveEntry();
			ConcentrationEntries.Remove(concentrationEntry3);
		}
	}

	public void RemoveEntry(BlueprintAbility ability, BaseUnitEntity target)
	{
		ConcentrationEntry concentrationEntry = ConcentrationEntries.FirstOrDefault((ConcentrationEntry p) => p.Ability == ability && p.Target == target);
		concentrationEntry?.RemoveEntry();
		ConcentrationEntries.Remove(concentrationEntry);
	}

	public void RemoveAllEntries()
	{
		ConcentrationEntry[] array = ConcentrationEntries.ToArray();
		foreach (ConcentrationEntry concentrationEntry in array)
		{
			concentrationEntry?.RemoveEntry();
			ConcentrationEntries.Remove(concentrationEntry);
		}
	}

	public bool HasConcentrationActions()
	{
		return ConcentrationEntries.Any();
	}

	public void OnAreaBeginUnloading()
	{
		RemoveAllEntries();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateActionPoints evt)
	{
		int num = 0;
		foreach (ConcentrationEntry concentrationEntry in ConcentrationEntries)
		{
			num += concentrationEntry.ActionPointCost;
		}
		if (num > AdditionalConcentrationActionPoints)
		{
			num -= AdditionalConcentrationActionPoints;
			evt.RegenBonus -= num;
		}
	}

	public void OnEventDidTrigger(RuleCalculateActionPoints evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateMovementPoints evt)
	{
		foreach (ConcentrationEntry concentrationEntry in ConcentrationEntries)
		{
			evt.Bonus -= concentrationEntry.MovementPointCost;
		}
	}

	public void OnEventDidTrigger(RuleCalculateMovementPoints evt)
	{
	}

	public void HandleUnitJoinCombat()
	{
	}

	public void HandleUnitLeaveCombat()
	{
		RemoveAllEntries();
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit == base.Owner)
		{
			RemoveAllEntries();
		}
		ConcentrationEntries.RemoveAll((ConcentrationEntry p) => p.Target == unit);
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit == base.Owner)
		{
			RemoveAllEntries();
		}
		ConcentrationEntries.RemoveAll((ConcentrationEntry p) => p.Target == unit);
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (base.Owner.State.IsHelpless || !base.Owner.State.IsAble || !base.Owner.State.CanAct)
		{
			RemoveAllEntries();
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		ConcentrationEntry concentrationEntry = ConcentrationEntries.FirstOrDefault((ConcentrationEntry p) => p.Buffs.Contains(buff));
		if (concentrationEntry != null)
		{
			concentrationEntry.Buffs.Remove(buff);
			concentrationEntry.RemoveEntry();
			ConcentrationEntries.Remove(concentrationEntry);
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		if (base.Owner.State.IsHelpless || !base.Owner.State.IsAble || !base.Owner.State.CanAct)
		{
			RemoveAllEntries();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<ConcentrationEntry> concentrationEntries = ConcentrationEntries;
		if (concentrationEntries != null)
		{
			for (int i = 0; i < concentrationEntries.Count; i++)
			{
				Hash128 val2 = ClassHasher<ConcentrationEntry>.GetHash128(concentrationEntries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
