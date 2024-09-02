using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartBonusAbility : BaseUnitPart, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>, IUnitCommandActHandler<EntitySubscriber>, IUnitCommandActHandler, IEventTag<IUnitCommandActHandler, EntitySubscriber>, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IInterruptTurnEndHandler<EntitySubscriber>, IInterruptTurnEndHandler, IEventTag<IInterruptTurnEndHandler, EntitySubscriber>, IHashable
{
	public class BonusAbilityData : IHashable
	{
		[JsonProperty]
		public int Count { get; set; }

		[JsonProperty]
		public int CostBonus { get; }

		[JsonProperty]
		public bool IgnoreMinimalCost { get; }

		[JsonProperty]
		public EntityFactSource Source { get; }

		[JsonProperty]
		public RestrictionsHolder.Reference Restrictions { get; }

		[JsonConstructor]
		public BonusAbilityData(int count, EntityFactSource source, int costBonus, RestrictionsHolder.Reference restrictions, bool ignoreMinimalCost)
		{
			Count = count;
			Source = source;
			CostBonus = costBonus;
			Restrictions = restrictions;
			IgnoreMinimalCost = ignoreMinimalCost;
		}

		public bool IsCorrectAbility(AbilityData data)
		{
			return Restrictions?.Get()?.IsPassed(new PropertyContext(data, data.Caster)) ?? true;
		}

		public override string ToString()
		{
			return $"Source={Source}, count={Count}, costBonus={CostBonus}, restrictions={Restrictions}";
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			int val = Count;
			result.Append(ref val);
			int val2 = CostBonus;
			result.Append(ref val2);
			bool val3 = IgnoreMinimalCost;
			result.Append(ref val3);
			Hash128 val4 = ClassHasher<EntityFactSource>.GetHash128(Source);
			result.Append(ref val4);
			Hash128 val5 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(Restrictions);
			result.Append(ref val5);
			return result;
		}
	}

	[JsonProperty]
	private readonly List<BonusAbilityData> m_Bonuses = new List<BonusAbilityData>();

	private static LogChannel Logger => BonusAbilityExtension.Logger;

	public bool HasBonusAbilityUsage(AbilityData ability)
	{
		return m_Bonuses.Any((BonusAbilityData x) => x.IsCorrectAbility(ability));
	}

	private BonusAbilityData GetBestBonusAbilityUsage(AbilityData ability)
	{
		BonusAbilityData bonusAbilityData = null;
		for (int i = 0; i < m_Bonuses.Count; i++)
		{
			BonusAbilityData bonusAbilityData2 = m_Bonuses[i];
			if (bonusAbilityData2.IsCorrectAbility(ability) && (bonusAbilityData == null || bonusAbilityData2.CostBonus < bonusAbilityData.CostBonus))
			{
				bonusAbilityData = bonusAbilityData2;
			}
		}
		return bonusAbilityData;
	}

	public void AddBonusAbility(EntityFactSource source, int count, int costBonus, RestrictionsHolder.Reference restrictions, bool ignoreMinimalCost = false)
	{
		BonusAbilityData bonusAbilityData = new BonusAbilityData(count, source, costBonus, restrictions, ignoreMinimalCost);
		m_Bonuses.Add(bonusAbilityData);
		Logger.Log($"Add bonus ability usage. {bonusAbilityData}. Owner={base.Owner}");
	}

	public void RemoveBonusAbility(BlueprintFact source)
	{
		m_Bonuses.RemoveAll((BonusAbilityData p) => p.Source.Blueprint == source);
	}

	private void Reset()
	{
		m_Bonuses.Clear();
		Logger.Log($"Reset bonus ability usages. Owner={base.Owner}");
		RemoveSelfIfEmpty();
	}

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
		BonusAbilityData bestBonusAbilityUsage = GetBestBonusAbilityUsage(evt.AbilityData);
		if (bestBonusAbilityUsage != null)
		{
			if (bestBonusAbilityUsage.IgnoreMinimalCost)
			{
				evt.CostBonusAfterMinimum += bestBonusAbilityUsage.CostBonus;
			}
			else
			{
				evt.CostBonus += bestBonusAbilityUsage.CostBonus;
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		Reset();
	}

	public void HandleUnitEndInterruptTurn()
	{
		Reset();
	}

	public void HandleUnitJoinCombat()
	{
		Reset();
	}

	public void HandleUnitLeaveCombat()
	{
		Reset();
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (evt.ForceFreeAction)
		{
			return;
		}
		BonusAbilityData bestBonusAbilityUsage = GetBestBonusAbilityUsage(evt.Spell);
		if (bestBonusAbilityUsage != null)
		{
			bestBonusAbilityUsage.Count--;
			if (bestBonusAbilityUsage.Count <= 0)
			{
				m_Bonuses.Remove(bestBonusAbilityUsage);
			}
			Logger.Log($"Reduce bonus ability usage count. {bestBonusAbilityUsage}. Owner={base.Owner}");
			RemoveSelfIfEmpty();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_Bonuses.Count == 0)
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<BonusAbilityData> bonuses = m_Bonuses;
		if (bonuses != null)
		{
			for (int i = 0; i < bonuses.Count; i++)
			{
				Hash128 val2 = ClassHasher<BonusAbilityData>.GetHash128(bonuses[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
