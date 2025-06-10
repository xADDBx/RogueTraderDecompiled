using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartCounterAttack : UnitPart, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, IUnitRunCommandHandler, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IHashable
{
	public class Entry : IHashable
	{
		[JsonProperty]
		public readonly string FactId;

		[JsonProperty]
		public readonly string ComponentId;

		[JsonProperty]
		public int UseCount { get; private set; }

		public int UsageLimit { get; private set; }

		public BaseUnitEntity Owner { get; private set; }

		public UnitFact Fact { get; private set; }

		public CounterAttack Component { get; private set; }

		public Cells MaxDistanceToAlly { get; private set; }

		public bool CanUse(Func<CounterAttack.TriggerType, bool> triggerFn)
		{
			if (UsageLimit != -1 && UseCount >= UsageLimit)
			{
				return false;
			}
			if (Component == null || !triggerFn(Component.Trigger))
			{
				return false;
			}
			return true;
		}

		[JsonConstructor]
		private Entry()
		{
		}

		public Entry(string factId, string componentId)
		{
			FactId = factId;
			ComponentId = componentId;
		}

		public void Setup(BaseUnitEntity owner, UnitFact fact, CounterAttack component, int limit)
		{
			Owner = owner;
			Fact = fact;
			Component = component;
			if (component.GuardAllies)
			{
				MaxDistanceToAlly = component.GuardAlliesRange.Calculate(Fact.MaybeContext).Cells();
			}
			UsageLimit = limit;
		}

		public void Use()
		{
			UseCount++;
		}

		public void ResetUse()
		{
			UseCount = 0;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(FactId);
			result.Append(ComponentId);
			int val = UseCount;
			result.Append(ref val);
			return result;
		}
	}

	[JsonProperty]
	private readonly List<Entry> m_Entries = new List<Entry>();

	private Action m_DelayedCounterAttackFn;

	public void Add(UnitFact fact, CounterAttack component, int limit)
	{
		Entry entry = m_Entries.Find((Entry i) => i.FactId == fact.UniqueId && i.ComponentId == component.name);
		if (entry == null)
		{
			entry = new Entry(fact.UniqueId, component.name);
			m_Entries.Add(entry);
		}
		entry.Setup(base.Owner, fact, component, limit);
	}

	public void Remove(UnitFact fact, CounterAttack component)
	{
		m_Entries.RemoveAll((Entry i) => i.FactId == fact.UniqueId && i.Component == component);
		if (m_Entries.Empty())
		{
			RemoveSelf();
		}
	}

	[CanBeNull]
	private Entry GetBestEntry([CanBeNull] Entry e1, [CanBeNull] Entry e2, Func<CounterAttack.TriggerType, bool> triggerFn)
	{
		bool flag = CanUse(e1, triggerFn);
		bool flag2 = CanUse(e2, triggerFn);
		if (!flag && !flag2)
		{
			return null;
		}
		if (flag && !flag2)
		{
			return e1;
		}
		if (!flag)
		{
			return e2;
		}
		bool flag3 = e1.UsageLimit != -1;
		bool flag4 = e2.UsageLimit != -1;
		if (!flag3 && flag4)
		{
			return e1;
		}
		if (flag3 && !flag4)
		{
			return e2;
		}
		return e1;
		static bool CanUse(Entry e, Func<CounterAttack.TriggerType, bool> t)
		{
			return e?.CanUse(t) ?? false;
		}
	}

	[CanBeNull]
	private Entry FindBestEntry(Func<CounterAttack.TriggerType, bool> triggerFn)
	{
		Entry entry = null;
		foreach (Entry entry2 in m_Entries)
		{
			if (entry2.Owner != null && entry2.Fact != null && entry2.Component != null)
			{
				entry = GetBestEntry(entry, entry2, triggerFn);
			}
		}
		return entry;
	}

	private BaseUnitEntity ComputeTargetAllyUnit(UnitUseAbility useAbilityCmd)
	{
		BaseUnitEntity baseUnitEntity = null;
		if (useAbilityCmd.Ability.GetPatternSettings() == null)
		{
			if (useAbilityCmd.TargetUnit != null && base.Owner.IsAlly(useAbilityCmd.TargetUnit) && useAbilityCmd.TargetUnit is BaseUnitEntity baseUnitEntity2)
			{
				baseUnitEntity = baseUnitEntity2;
			}
		}
		else
		{
			Vector3 vector3Position = useAbilityCmd.Ability.GetBestShootingPosition(useAbilityCmd.Target).Vector3Position;
			foreach (CustomGridNodeBase node in useAbilityCmd.Ability.GetPattern(useAbilityCmd.Target, vector3Position).Nodes)
			{
				BaseUnitEntity unit = node.GetUnit();
				if (unit != null && base.Owner.IsAlly(unit) && (baseUnitEntity == null || base.Owner.DistanceTo(unit) < base.Owner.DistanceTo(baseUnitEntity)))
				{
					baseUnitEntity = unit;
				}
			}
		}
		return baseUnitEntity;
	}

	private (Entry, BaseUnitEntity) FindBestEntryWithAlliesOnly(Func<CounterAttack.TriggerType, bool> triggerFn, UnitUseAbility useAbilityCmd)
	{
		BaseUnitEntity baseUnitEntity = null;
		bool flag = false;
		Entry entry = null;
		foreach (Entry entry2 in m_Entries)
		{
			if (entry2.Owner == null || entry2.Fact == null || entry2.Component == null || !entry2.Component.GuardAllies)
			{
				continue;
			}
			int value = entry2.MaxDistanceToAlly.Value;
			if (value > 0)
			{
				if (!flag)
				{
					baseUnitEntity = ComputeTargetAllyUnit(useAbilityCmd);
					flag = true;
				}
				if (base.Owner.DistanceToInCells(baseUnitEntity) <= value)
				{
					entry = GetBestEntry(entry, entry2, triggerFn);
				}
			}
		}
		return ValueTuple.Create(entry, baseUnitEntity);
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		if (m_DelayedCounterAttackFn != null || base.Owner.Commands.Queue.Any((UnitCommandParams x) => x is UnitAttackOfOpportunityParams))
		{
			return;
		}
		Entry entry = FindBestEntry(ShouldTrigger);
		if (entry == null || !entry.Component.Restriction.IsPassed(new PropertyContext(entry.Fact, evt.InitiatorUnit, evt, evt.Ability)))
		{
			return;
		}
		m_DelayedCounterAttackFn = delegate
		{
			if (Game.Instance.AttackOfOpportunityController.Provoke(evt.InitiatorUnit, base.Owner, entry.Fact, entry.Component.CanUseInRange, canMove: false) != null)
			{
				entry.Use();
			}
		};
		bool ShouldTrigger(CounterAttack.TriggerType componentTrigger)
		{
			return componentTrigger switch
			{
				CounterAttack.TriggerType.AfterDodgeAttack => evt.ResultDodgeRule?.Result ?? false, 
				CounterAttack.TriggerType.AfterParryAttack => evt.ResultParryRule?.Result ?? false, 
				CounterAttack.TriggerType.AfterBlockAttack => evt.ResultBlockRule?.Result ?? false, 
				CounterAttack.TriggerType.AfterAnyAttack => true, 
				_ => false, 
			};
		}
	}

	public void HandleUnitRunCommand(AbstractUnitCommand cmd)
	{
		if (!(cmd is UnitUseAbility unitUseAbility) || base.Owner.IsDeadOrUnconscious || !unitUseAbility.Ability.Blueprint.AttackType.HasValue)
		{
			return;
		}
		if (cmd.Executor != base.Owner && cmd.Executor is BaseUnitEntity target)
		{
			Entry entry2 = FindBestEntry((CounterAttack.TriggerType componentTrigger) => componentTrigger == CounterAttack.TriggerType.BeforeAttack);
			if (entry2 != null && ComputeTargetAllyUnit(unitUseAbility) == base.Owner && Rulebook.Trigger(new RuleCalculateCounterAttackChance(base.Owner, target)).Result > 0)
			{
				UnitCommandHandle unitCommandHandle = Game.Instance.AttackOfOpportunityController.Provoke(target, base.Owner, entry2.Fact, entry2.Component.CanUseInRange, canMove: false);
				if (unitCommandHandle != null)
				{
					cmd.BlockOn(unitCommandHandle.Cmd);
					entry2.Use();
				}
			}
		}
		if (cmd.Executor == base.Owner || cmd.TargetUnit == base.Owner)
		{
			return;
		}
		var (entry, currentTarget) = FindBestEntryWithAlliesOnly((CounterAttack.TriggerType componentTrigger) => componentTrigger == CounterAttack.TriggerType.AfterAnyAttack, unitUseAbility);
		if (entry == null || !entry.Component.GuardAlliesRestriction.IsPassed(new PropertyContext(base.Owner, null, currentTarget)))
		{
			return;
		}
		m_DelayedCounterAttackFn = delegate
		{
			if (cmd.Executor is BaseUnitEntity target2 && Game.Instance.AttackOfOpportunityController.Provoke(target2, base.Owner, entry.Fact, entry.Component.CanUseInRange, entry.Component.GuardAlliesCanMove) != null)
			{
				entry.Use();
			}
		};
		if (entry.Component.Trigger == CounterAttack.TriggerType.BeforeAttack)
		{
			m_DelayedCounterAttackFn();
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand cmd)
	{
		if (m_DelayedCounterAttackFn != null)
		{
			m_DelayedCounterAttackFn();
			m_DelayedCounterAttackFn = null;
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			return;
		}
		foreach (Entry entry in m_Entries)
		{
			entry.ResetUse();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Entry> entries = m_Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<Entry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
