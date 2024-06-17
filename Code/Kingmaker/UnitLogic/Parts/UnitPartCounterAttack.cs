using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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

public class UnitPartCounterAttack : UnitPart, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, IUnitRunCommandHandler, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, IHashable
{
	public class Entry : IHashable
	{
		[JsonProperty]
		public readonly string FactId;

		[JsonProperty]
		public readonly string ComponentId;

		[JsonProperty]
		public int? UsageLimit { get; private set; }

		[JsonProperty]
		public int UseCount { get; private set; }

		public BaseUnitEntity Owner { get; private set; }

		public UnitFact Fact { get; private set; }

		public CounterAttack Component { get; private set; }

		public Cells MaxDistanceToAlly { get; private set; }

		public bool CanUse(CounterAttack.TriggerType trigger, [CanBeNull] BaseUnitEntity targetAlly)
		{
			if (Owner == null || Fact == null || Component == null)
			{
				return false;
			}
			if (UseCount >= UsageLimit)
			{
				return false;
			}
			CounterAttack component = Component;
			if (component != null && component.Trigger < trigger)
			{
				return false;
			}
			int value = MaxDistanceToAlly.Value;
			if (targetAlly != null)
			{
				if (!Component.GuardAllies)
				{
					return false;
				}
				if (value > 0 && Owner.DistanceToInCells(targetAlly) > value)
				{
					return false;
				}
			}
			return true;
		}

		[JsonConstructor]
		private Entry()
		{
		}

		public Entry(BaseUnitEntity owner, UnitFact fact, CounterAttack component)
		{
			FactId = fact.UniqueId;
			Component = component;
			Setup(owner, fact, component);
		}

		public void Setup(BaseUnitEntity owner, UnitFact fact, CounterAttack component)
		{
			Owner = owner;
			Fact = fact;
			Component = component;
			if (component.GuardAllies)
			{
				MaxDistanceToAlly = component.GuardAlliesRange.Calculate(Fact.MaybeContext).Cells();
			}
		}

		public void Use()
		{
			if (UsageLimit.HasValue)
			{
				UseCount++;
			}
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(FactId);
			result.Append(ComponentId);
			if (UsageLimit.HasValue)
			{
				int val = UsageLimit.Value;
				result.Append(ref val);
			}
			int val2 = UseCount;
			result.Append(ref val2);
			return result;
		}
	}

	[JsonProperty]
	private readonly List<Entry> m_Entries = new List<Entry>();

	private Entry m_DelayedCounterAttackEntry;

	public void Add(UnitFact fact, CounterAttack component)
	{
		Entry entry = m_Entries.Find((Entry i) => i.FactId == fact.UniqueId && i.ComponentId == component.name);
		if (entry != null)
		{
			entry.Setup(base.Owner, fact, component);
		}
		else
		{
			m_Entries.Add(new Entry(base.Owner, fact, component));
		}
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
	private Entry GetBestEntry([CanBeNull] Entry e1, [CanBeNull] Entry e2, CounterAttack.TriggerType trigger, [CanBeNull] BaseUnitEntity targetAlly)
	{
		bool flag = CanUse(e1, trigger, targetAlly);
		bool flag2 = CanUse(e2, trigger, targetAlly);
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
		bool hasValue = e1.UsageLimit.HasValue;
		bool hasValue2 = e2.UsageLimit.HasValue;
		if (!hasValue && hasValue2)
		{
			return e1;
		}
		if (hasValue && !hasValue2)
		{
			return e2;
		}
		return e1;
		static bool CanUse(Entry e, CounterAttack.TriggerType t, [CanBeNull] BaseUnitEntity ally)
		{
			return e?.CanUse(t, ally) ?? false;
		}
	}

	[CanBeNull]
	private Entry FindBestEntry(CounterAttack.TriggerType trigger, [CanBeNull] BaseUnitEntity targetAlly)
	{
		Entry entry = null;
		foreach (Entry entry2 in m_Entries)
		{
			entry = GetBestEntry(entry, entry2, trigger, targetAlly);
		}
		return entry;
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		if (!evt.IsMelee)
		{
			return;
		}
		CounterAttack.TriggerType trigger = ((evt.Result != AttackResult.Parried) ? CounterAttack.TriggerType.AfterAnyAttack : CounterAttack.TriggerType.AfterParryAttack);
		Entry entry = FindBestEntry(trigger, null);
		if (entry != null)
		{
			if (Game.Instance.AttackOfOpportunityController.Provoke(evt.InitiatorUnit, base.Owner, entry.Fact))
			{
				entry.Use();
			}
			else if (entry.Component.CanUseInRange && TryCounterAttackInRange(evt.InitiatorUnit, base.Owner))
			{
				entry.Use();
			}
		}
	}

	public void HandleUnitRunCommand(AbstractUnitCommand cmd)
	{
		if (!(cmd is UnitUseAbility unitUseAbility) || unitUseAbility.Ability.Blueprint.AttackType != AttackAbilityType.Melee || cmd.Executor == base.Owner || cmd.TargetUnit == base.Owner)
		{
			return;
		}
		AbilityData ability = unitUseAbility.Ability;
		BaseUnitEntity baseUnitEntity = null;
		if (ability.GetPatternSettings() == null)
		{
			if (cmd.TargetUnit != null && base.Owner.IsAlly(cmd.TargetUnit) && cmd.TargetUnit is BaseUnitEntity baseUnitEntity2)
			{
				baseUnitEntity = baseUnitEntity2;
			}
		}
		else
		{
			Vector3 vector3Position = ability.GetBestShootingPosition(cmd.Target).Vector3Position;
			foreach (CustomGridNodeBase node in ability.GetPattern(cmd.Target, vector3Position).Nodes)
			{
				BaseUnitEntity unit = node.GetUnit();
				if (unit != null && base.Owner.IsAlly(unit) && (baseUnitEntity == null || base.Owner.DistanceTo(unit) < base.Owner.DistanceTo(baseUnitEntity)))
				{
					baseUnitEntity = unit;
				}
			}
		}
		if (baseUnitEntity != null)
		{
			m_DelayedCounterAttackEntry = FindBestEntry(CounterAttack.TriggerType.AfterAnyAttack, baseUnitEntity);
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand cmd)
	{
		if (m_DelayedCounterAttackEntry != null)
		{
			if (cmd.Executor is BaseUnitEntity target && Game.Instance.AttackOfOpportunityController.Provoke(target, base.Owner, m_DelayedCounterAttackEntry.Fact))
			{
				m_DelayedCounterAttackEntry.Use();
			}
			else if (m_DelayedCounterAttackEntry.Component.CanUseInRange && TryCounterAttackInRange(cmd.Executor, base.Owner))
			{
				m_DelayedCounterAttackEntry.Use();
			}
			m_DelayedCounterAttackEntry = null;
		}
	}

	public void HandleUnitFinishedCommand()
	{
	}

	private bool TryCounterAttackInRange(AbstractUnitEntity target, BaseUnitEntity attacker)
	{
		if (!attacker.IsEnemy(target))
		{
			return false;
		}
		if (target.LifeState.IsDead)
		{
			return false;
		}
		if (!attacker.CombatState.CanActInCombat || !attacker.State.CanAct)
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = attacker.GetThreatHandMelee()?.Weapon;
		if (itemEntityWeapon == null)
		{
			return false;
		}
		Ability ability2 = itemEntityWeapon.Abilities.FirstItem((Ability ability) => ability.Data.IsMelee);
		if (ability2 == null)
		{
			PFLog.Default.Error("No abilities in blueprint ranged weapon " + itemEntityWeapon.Name + " for counter attack (unit " + attacker.Name + ")");
			return false;
		}
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability2.Data, target)
		{
			IgnoreCooldown = true,
			FreeAction = true,
			NeedLoS = false,
			IgnoreAbilityUsingInThreateningArea = true
		};
		attacker.Commands.AddToQueue(cmdParams);
		return true;
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
