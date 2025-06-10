using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartHealth : MechanicEntityPart, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, IHashable
{
	public interface IOwner : IEntityPartOwner<PartHealth>, IEntityPartOwner
	{
		PartHealth Health { get; }
	}

	public class TemporaryHitPointsData : IHashable
	{
		[JsonProperty]
		public readonly EntityFactRef<Buff> Source;

		[JsonProperty]
		public int Round { get; set; }

		[JsonProperty]
		public int Value { get; set; }

		[JsonConstructor]
		private TemporaryHitPointsData()
		{
		}

		public TemporaryHitPointsData([NotNull] Buff source)
		{
			Source = source;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityFactRef<Buff> obj = Source;
			Hash128 val = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
			result.Append(ref val);
			int val2 = Round;
			result.Append(ref val2);
			int val3 = Value;
			result.Append(ref val3);
			return result;
		}
	}

	[JsonProperty]
	private int m_DamageReceivedThisTurn;

	[JsonProperty]
	private int m_LastTurnWhenReceiveDamage;

	[JsonProperty]
	private int m_ConsecutiveOutOfCombatTurnsWithoutDamage;

	[JsonProperty]
	[CanBeNull]
	private List<TemporaryHitPointsData> m_TemporaryHitPoints = new List<TemporaryHitPointsData>();

	[JsonProperty]
	private float m_MissingHpFraction;

	[JsonIgnore]
	public bool DiscardTrauma;

	[CanBeNull]
	private List<(EntityFact Fact, BlueprintComponent Component, int Value)> m_HealthGuards;

	private int m_MinHitPoints;

	public int Damage
	{
		get
		{
			return Mathf.FloorToInt(m_MissingHpFraction * (float)MaxHitPoints);
		}
		private set
		{
			m_MissingHpFraction = (float)value / (float)MaxHitPoints;
		}
	}

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueAttributeStat Toughness => StatsContainer.GetAttribute(StatType.WarhammerToughness);

	public ModifiableValueHitPoints HitPoints => StatsContainer.GetStat<ModifiableValueHitPoints>(StatType.HitPoints);

	[CanBeNull]
	public RuleDealDamage LastHandledDamage { get; set; }

	public int HitPointsLeft => (int)HitPoints - Damage;

	public int MaxHitPoints => HitPoints;

	public int WoundFreshStacks => base.Owner.Buffs.Get(Root.WH.BlueprintTraumaRoot.FreshWound)?.Rank ?? 0;

	public int WoundOldStacks => base.Owner.Buffs.Get(Root.WH.BlueprintTraumaRoot.OldWound)?.Rank ?? 0;

	public int TraumaStacks => base.Owner.Buffs.Get(Root.WH.BlueprintTraumaRoot.Trauma)?.Rank ?? 0;

	public bool TraumaIsAvailable
	{
		get
		{
			if (base.Owner is AbstractUnitEntity abstractUnitEntity)
			{
				return !(abstractUnitEntity is StarshipEntity);
			}
			return false;
		}
	}

	public int TemporaryHitPoints => CalculateTemporaryHitPoints();

	public int MinHitPoints => Math.Max(m_MinHitPoints, base.Owner.HasMechanicFeature(MechanicsFeatureType.Undying) ? 1 : 0);

	protected override void OnAttach()
	{
		Initialize();
	}

	protected override void OnPrePostLoad()
	{
		Initialize();
	}

	void IInGameHandler.HandleObjectInGameChanged()
	{
		LastHandledDamage = null;
	}

	private void Initialize()
	{
		StatsContainer.RegisterAttribute(StatType.WarhammerToughness);
		StatsContainer.Register<ModifiableValueHitPoints>(StatType.HitPoints);
	}

	public void SetDamage(int damage)
	{
		if (damage < 0)
		{
			PFLog.Default.Error("Damage can't be less than 0");
			return;
		}
		bool flag = base.Owner.HasMechanicFeature(MechanicsFeatureType.Undying);
		int num = Math.Max(m_MinHitPoints, flag ? 1 : 0);
		int damage2 = Damage;
		int hitPointsLeft = HitPointsLeft;
		Damage = Math.Clamp(damage, 0, Math.Max(0, MaxHitPoints - num));
		if (Damage == damage2)
		{
			return;
		}
		if (Damage > damage2)
		{
			m_LastTurnWhenReceiveDamage = Game.Instance.TurnController.GameRound;
			m_ConsecutiveOutOfCombatTurnsWithoutDamage = 0;
			if (TraumaIsAvailable)
			{
				AddWoundsAndTraumasIfNecessary(hitPointsLeft, damage2);
			}
		}
		if (base.Owner.View is AbstractUnitEntityView abstractUnitEntityView)
		{
			abstractUnitEntityView.HandleDamage();
		}
		base.Owner.GetDestructionStagesManagerOptional()?.Update();
	}

	public void DealDamage(int damage)
	{
		SetDamage(Damage + ApplyTemporaryHitPoints(damage));
	}

	public void HealDamage(int heal)
	{
		SetDamage(Math.Max(0, Damage - heal));
	}

	public void HealDamageAll()
	{
		SetDamage(0);
	}

	public void SetHitPointsLeft(int targetHP)
	{
		SetDamage(Math.Max(0, MaxHitPoints - targetHP));
	}

	public void HealAll()
	{
		HealDamageAll();
		HealFreshWound(int.MaxValue);
		HealOldWound(int.MaxValue);
		HealTrauma(int.MaxValue);
	}

	public void DealWounds(int count)
	{
		if ((bool)base.Owner.Features.FreshInjuryImmunity)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitWoundHandler>)delegate(IUnitWoundHandler h)
			{
				h.HandleWoundAvoided();
			}, isCheckRuntime: true);
		}
		else
		{
			DealWoundsImpl(count);
		}
	}

	private void DealWoundsImpl(int count)
	{
		if (base.Owner is BaseUnitEntity { IsPet: not false })
		{
			return;
		}
		int num = (base.Owner.IsInPlayerParty ? Game.Instance.Player.TraumasModification.WoundStacksForTraumaModifier : 0);
		if (!DiscardTrauma && WoundFreshStacks + WoundOldStacks + 1 >= SettingsRoot.Difficulty.WoundStacksForTrauma.GetValue() + num)
		{
			DealTraumas(1);
			return;
		}
		base.Owner.Buffs.Add(Root.WH.BlueprintTraumaRoot.FreshWound)?.AddRank(count - 1);
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitWoundHandler>)delegate(IUnitWoundHandler h)
		{
			h.HandleWoundReceived();
		}, isCheckRuntime: true);
	}

	public void DealTraumas(int count)
	{
		if ((bool)base.Owner.Features.OldInjuryImmunity)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitTraumaHandler>)delegate(IUnitTraumaHandler h)
			{
				h.HandleTraumaAvoided();
			}, isCheckRuntime: true);
		}
		else
		{
			DealTraumasImpl(count);
		}
	}

	private void DealTraumasImpl(int count)
	{
		if (!(base.Owner is BaseUnitEntity { IsPet: not false }))
		{
			base.Owner.Buffs.Remove(Root.WH.BlueprintTraumaRoot.FreshWound);
			base.Owner.Buffs.Remove(Root.WH.BlueprintTraumaRoot.OldWound);
			base.Owner.Buffs.Add(Root.WH.BlueprintTraumaRoot.Trauma)?.AddRank(count - 1);
			EventBus.RaiseEvent((IMechanicEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitTraumaHandler>)delegate(IUnitTraumaHandler h)
			{
				h.HandleTraumaReceived();
			}, isCheckRuntime: true);
		}
	}

	private void AddWoundsAndTraumasIfNecessary(int prevHPLeft, int prevDamage)
	{
		if (!DiscardTrauma && prevHPLeft > 0 && HitPointsLeft <= 0)
		{
			DealTraumas(1);
			return;
		}
		DiscardTrauma = false;
		int damageReceivedThisTurn = m_DamageReceivedThisTurn;
		m_DamageReceivedThisTurn += Damage - prevDamage;
		int woundDamagePerTurnThresholdHPFractionModifier = Game.Instance.Player.TraumasModification.WoundDamagePerTurnThresholdHPFractionModifier;
		float num = (base.Owner.IsInPlayerParty ? (Math.Max(0f, (int)SettingsRoot.Difficulty.WoundDamagePerTurnThresholdHPFraction + woundDamagePerTurnThresholdHPFractionModifier) / 100f) : 0.5f);
		int num2 = Mathf.CeilToInt((float)MaxHitPoints * num);
		if ((damageReceivedThisTurn < num2 && m_DamageReceivedThisTurn >= num2) || (bool)ContextData<PartHealthExtension.IgnoreWoundThreshold>.Current)
		{
			DealWounds(1);
			m_DamageReceivedThisTurn = 0;
		}
		DiscardTrauma = false;
	}

	public void UpdateWoundsAndTraumasOnNewTurn(bool isTurnBased)
	{
		m_DamageReceivedThisTurn = 0;
		Buff buff = base.Owner.Buffs.GetBuff(Root.WH.BlueprintTraumaRoot.FreshWound);
		if (buff != null)
		{
			if (!isTurnBased)
			{
				m_ConsecutiveOutOfCombatTurnsWithoutDamage++;
			}
			int num = (base.Owner.IsInPlayerParty ? Game.Instance.Player.TraumasModification.OldWoundDelayRoundsModifier : 0);
			int num2 = SettingsRoot.Difficulty.OldWoundDelayRounds.GetValue() + num;
			if (isTurnBased && m_ConsecutiveOutOfCombatTurnsWithoutDamage > num2)
			{
				int rank = buff.Rank;
				buff.Remove();
				m_ConsecutiveOutOfCombatTurnsWithoutDamage = 0;
				base.Owner.Buffs.Add(Root.WH.BlueprintTraumaRoot.OldWound)?.AddRank(rank - 1);
			}
		}
	}

	public void HealFreshWound(int count)
	{
		Buff buff = base.Owner.Buffs.Get(Root.WH.BlueprintTraumaRoot.FreshWound);
		if (buff == null)
		{
			return;
		}
		if (buff.Rank <= count)
		{
			EventBus.RaiseEvent((IEntity)base.Owner, (Action<IHealWoundOrTrauma>)delegate(IHealWoundOrTrauma h)
			{
				h.HandleOnHealWoundOrTrauma(buff);
			}, isCheckRuntime: true);
		}
		buff.RemoveRank(count);
	}

	public void HealOldWound(int count)
	{
		Buff buff = base.Owner.Buffs.Get(Root.WH.BlueprintTraumaRoot.OldWound);
		if (buff == null)
		{
			return;
		}
		if (buff.Rank <= count)
		{
			EventBus.RaiseEvent((IEntity)base.Owner, (Action<IHealWoundOrTrauma>)delegate(IHealWoundOrTrauma h)
			{
				h.HandleOnHealWoundOrTrauma(buff);
			}, isCheckRuntime: true);
		}
		buff.RemoveRank(count);
	}

	public void HealTrauma(int count)
	{
		Buff buff = base.Owner.Buffs.Get(Root.WH.BlueprintTraumaRoot.Trauma);
		if (buff == null)
		{
			return;
		}
		if (buff.Rank <= count)
		{
			EventBus.RaiseEvent((IEntity)base.Owner, (Action<IHealWoundOrTrauma>)delegate(IHealWoundOrTrauma h)
			{
				h.HandleOnHealWoundOrTrauma(buff);
			}, isCheckRuntime: true);
		}
		buff.RemoveRank(count);
	}

	private void UpdateMinHitPoints()
	{
		m_MinHitPoints = 0;
		if (m_HealthGuards == null)
		{
			return;
		}
		foreach (var healthGuard in m_HealthGuards)
		{
			int val = Math.Clamp(healthGuard.Value, 0, MaxHitPoints);
			m_MinHitPoints = Math.Max(m_MinHitPoints, val);
		}
		if (HitPointsLeft < m_MinHitPoints)
		{
			SetHitPointsLeft(m_MinHitPoints);
		}
	}

	public void AddHealthGuard(EntityFact fact, BlueprintComponent component, int value)
	{
		if (m_HealthGuards == null)
		{
			m_HealthGuards = new List<(EntityFact, BlueprintComponent, int)>();
		}
		m_HealthGuards.Add((fact, component, value));
		UpdateMinHitPoints();
	}

	public void RemoveHealthGuard(EntityFact fact, BlueprintComponent component)
	{
		m_HealthGuards?.RemoveAll(((EntityFact Fact, BlueprintComponent Component, int Value) i) => i.Fact == fact && i.Component == component);
		m_HealthGuards = (m_HealthGuards.Empty() ? null : m_HealthGuards);
		UpdateMinHitPoints();
	}

	private int CalculateTemporaryHitPoints()
	{
		if (m_TemporaryHitPoints == null)
		{
			return 0;
		}
		TemporaryHitPointsData temporaryHitPointsData = m_TemporaryHitPoints.MaxBy((TemporaryHitPointsData i) => i.Value);
		int num = 0;
		foreach (TemporaryHitPointsData temporaryHitPoint in m_TemporaryHitPoints)
		{
			if (temporaryHitPoint != temporaryHitPointsData && temporaryHitPoint.Round == Game.Instance.TurnController.GameRound)
			{
				num += temporaryHitPoint.Value;
			}
		}
		int num2 = temporaryHitPointsData?.Value ?? 0;
		if ((bool)base.Owner.Features.Vanguard)
		{
			return num2 + num;
		}
		return num2;
	}

	private int ApplyTemporaryHitPoints(int damage)
	{
		if (m_TemporaryHitPoints == null || m_TemporaryHitPoints.Empty())
		{
			return damage;
		}
		int num = damage;
		if ((bool)base.Owner.Features.Vanguard)
		{
			while (true)
			{
				TemporaryHitPointsData temporaryHitPointsData = GetStackingHP();
				if (temporaryHitPointsData == null || damage <= 0)
				{
					break;
				}
				Handle(temporaryHitPointsData, ref damage);
			}
		}
		if (m_TemporaryHitPoints == null)
		{
			return damage;
		}
		foreach (TemporaryHitPointsData item in m_TemporaryHitPoints.ToTempList())
		{
			if (!base.Owner.Features.Vanguard || item.Round != Game.Instance.TurnController.GameRound)
			{
				int damageValue2 = num;
				Handle(item, ref damageValue2);
				damage = Math.Min(damageValue2, damage);
			}
		}
		return damage;
		TemporaryHitPointsData GetStackingHP()
		{
			return m_TemporaryHitPoints?.Where((TemporaryHitPointsData i) => i.Round == Game.Instance.TurnController.GameRound).MaxBy((TemporaryHitPointsData i) => i.Value);
		}
		void Handle(TemporaryHitPointsData tHP, ref int damageValue)
		{
			int value = tHP.Value;
			int num2 = damageValue - value;
			tHP.Value = ((num2 < 0) ? (-num2) : 0);
			damageValue = Math.Max(0, num2);
			if (tHP.Value <= 0)
			{
				m_TemporaryHitPoints?.Remove(tHP);
				tHP.Source.Fact?.Remove();
			}
		}
	}

	public void AddTemporaryHitPoints(int amount, Buff sourceBuff)
	{
		if (!m_TemporaryHitPoints.HasItem((TemporaryHitPointsData i) => i.Source == sourceBuff) && amount > 0)
		{
			TemporaryHitPointsData item = new TemporaryHitPointsData(sourceBuff)
			{
				Value = amount,
				Round = Game.Instance.TurnController.GameRound
			};
			if (m_TemporaryHitPoints == null)
			{
				m_TemporaryHitPoints = new List<TemporaryHitPointsData>();
			}
			m_TemporaryHitPoints.Add(item);
			EventBus.RaiseEvent((IEntity)base.Owner, (Action<ITemporaryHitPoints>)delegate(ITemporaryHitPoints h)
			{
				h.HandleOnAddTemporaryHitPoints(amount, sourceBuff);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveTemporaryHitPoints(Buff sourceBuff)
	{
		if (!m_TemporaryHitPoints.Empty() && sourceBuff != null)
		{
			EventBus.RaiseEvent((IEntity)base.Owner, (Action<ITemporaryHitPoints>)delegate(ITemporaryHitPoints h)
			{
				h.HandleOnRemoveTemporaryHitPoints(GetTemporaryHitPointFromBuff(sourceBuff.Blueprint), sourceBuff);
			}, isCheckRuntime: true);
		}
		m_TemporaryHitPoints?.Remove((TemporaryHitPointsData i) => i.Source == sourceBuff);
		if (m_TemporaryHitPoints.Empty())
		{
			m_TemporaryHitPoints = null;
		}
	}

	public int GetTemporaryHitPointFromBuff([NotNull] BlueprintBuff sourceBuff)
	{
		if (m_TemporaryHitPoints != null && !m_TemporaryHitPoints.Empty())
		{
			return m_TemporaryHitPoints.Where((TemporaryHitPointsData p1) => p1.Source.Fact?.Blueprint == sourceBuff).Sum((TemporaryHitPointsData p2) => p2.Value);
		}
		return 0;
	}

	public void CleanupTemporaryHitPoints()
	{
		m_TemporaryHitPoints?.Clear();
	}

	public static void RestUnit(BaseUnitEntity unit)
	{
		if (unit.LifeState.IsDead)
		{
			unit.LifeState.Resurrect();
			unit.Position = Game.Instance.Player.MainCharacter.Entity.Position;
		}
		Rulebook.Trigger(new RuleHealDamage(unit, unit, default(DiceFormula), unit.Health.HitPoints.ModifiedValue));
		unit.CombatState.LastStraightMoveLength = 0;
		unit.CombatState.LastDiagonalCount = 0;
		unit.CombatState.ResetActionPointsAll();
		unit.CombatState.AttackInRoundCount = 0;
		unit.CombatState.AttackedInRoundCount = 0;
		unit.CombatState.HitInRoundCount = 0;
		unit.CombatState.GotHitInRoundCount = 0;
		unit.GetAbilityCooldownsOptional()?.Clear();
		unit.GetTwoWeaponFightingOptional()?.ResetAttacks();
		TryResetDebuffs(unit);
		foreach (ModifiableValueAttributeStat attribute in unit.Attributes)
		{
			attribute.Damage = 0;
			attribute.Drain = 0;
		}
		unit.Health.HealAll();
	}

	private static void TryResetDebuffs(BaseUnitEntity unit)
	{
		DebuffSkillCheckRoot debuffSkillCheckRoot = BlueprintWarhammerRoot.Instance.SkillCheckRoot.DebuffSkillCheckRoot;
		unit.Buffs.Remove(debuffSkillCheckRoot.Fatigued);
		unit.Buffs.Remove(debuffSkillCheckRoot.Disturbed);
		unit.Buffs.Remove(debuffSkillCheckRoot.Perplexed);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_DamageReceivedThisTurn);
		result.Append(ref m_LastTurnWhenReceiveDamage);
		result.Append(ref m_ConsecutiveOutOfCombatTurnsWithoutDamage);
		List<TemporaryHitPointsData> temporaryHitPoints = m_TemporaryHitPoints;
		if (temporaryHitPoints != null)
		{
			for (int i = 0; i < temporaryHitPoints.Count; i++)
			{
				Hash128 val2 = ClassHasher<TemporaryHitPointsData>.GetHash128(temporaryHitPoints[i]);
				result.Append(ref val2);
			}
		}
		result.Append(ref m_MissingHpFraction);
		return result;
	}
}
