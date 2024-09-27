using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformMomentumChange : RulebookOptionalTargetEvent
{
	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager();

	[CanBeNull]
	private readonly MomentumGroup m_Group;

	private readonly int m_Value;

	public readonly MomentumChangeReason ChangeReason;

	private float m_ResolveLostBase;

	private int m_EntityResolve;

	private float m_Factor;

	private int m_KillerResolve;

	private int m_ResolvesGainFlat;

	private float m_ResolvesGain;

	private int m_InitiatorResolve;

	[CanBeNull]
	public MomentumGroup ResultGroup { get; private set; }

	public int ResultDeltaValueBase { get; private set; }

	public int ResultDeltaValue { get; private set; }

	public int ResultPrevValue { get; private set; }

	public int ResultCurrentValue { get; private set; }

	public int FlatBonus { get; set; }

	public int RawValue => m_Value;

	public float ResolveLostBase => m_ResolveLostBase;

	public int EntityResolve => m_EntityResolve;

	public float Factor => m_Factor;

	public int KillerResolve => m_KillerResolve;

	public float ResolvesGain => m_ResolvesGain;

	public int ResolvesGainFlat => m_ResolvesGainFlat;

	public int InitiatorResolve => m_InitiatorResolve;

	public static RulePerformMomentumChange CreateBecameDeadOrUnconscious([NotNull] MechanicEntity entity)
	{
		return new RulePerformMomentumChange(entity, null, null, 0, MomentumChangeReason.FallDeadOrUnconscious);
	}

	public static RulePerformMomentumChange CreateKillEnemy([NotNull] MechanicEntity killer, [NotNull] MechanicEntity enemy, [NotNull] MomentumGroup group)
	{
		return new RulePerformMomentumChange(killer, enemy, group, 0, MomentumChangeReason.KillEnemy);
	}

	public static RulePerformMomentumChange CreateStartTurn([NotNull] MechanicEntity entity)
	{
		return new RulePerformMomentumChange(entity, null, null, 0, MomentumChangeReason.StartTurn);
	}

	public static RulePerformMomentumChange CreateAbilityCost([NotNull] MechanicEntity entity, int cost, in RuleReason reason)
	{
		return new RulePerformMomentumChange(entity, null, null, cost, MomentumChangeReason.AbilityCost)
		{
			Reason = reason
		};
	}

	public static RulePerformMomentumChange CreateWound([NotNull] MechanicEntity entity)
	{
		return new RulePerformMomentumChange(entity, null, null, 0, MomentumChangeReason.Wound);
	}

	public static RulePerformMomentumChange CreateTrauma([NotNull] MechanicEntity entity)
	{
		return new RulePerformMomentumChange(entity, null, null, 0, MomentumChangeReason.Trauma);
	}

	public static RulePerformMomentumChange CreateCustom([NotNull] MechanicEntity entity, int value, in RuleReason reason)
	{
		return new RulePerformMomentumChange(entity, null, null, value, MomentumChangeReason.Custom)
		{
			Reason = reason
		};
	}

	public static RulePerformMomentumChange CreatePsychicPhenomena([NotNull] MechanicEntity entity, int cost)
	{
		return new RulePerformMomentumChange(entity, null, null, cost, MomentumChangeReason.PsychicPhenomena);
	}

	private RulePerformMomentumChange([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] MomentumGroup group, int value, MomentumChangeReason reason)
		: base(initiator, target)
	{
		base.HasNoTarget = target == null;
		ChangeReason = reason;
		m_Group = group;
		m_Value = value;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		MomentumGroup group = Game.Instance.TurnController.MomentumController.GetGroup(base.ConcreteInitiator);
		switch (ChangeReason)
		{
		case MomentumChangeReason.FallDeadOrUnconscious:
			ResultDeltaValueBase = -GetResolveLostForWoundOrTrauma(base.ConcreteInitiator, 0.5f);
			break;
		case MomentumChangeReason.KillEnemy:
			if (MaybeTarget != null && !MaybeTarget.IsAlly(base.Initiator))
			{
				group = m_Group;
				m_KillerResolve = base.ConcreteInitiator.GetStatOptional(StatType.Resolve);
				m_ResolvesGain = (MaybeTarget.IsPlayerFaction ? UnitDifficultyMomentumHelper.ResolvesGainedForPartyMemberKill : UnitDifficultyMomentumHelper.GetResolveGained(MaybeTarget));
				m_ResolvesGain = (MaybeTarget.IsPlayerFaction ? UnitDifficultyMomentumHelper.ResolvesGainedForPartyMemberKill : UnitDifficultyMomentumHelper.GetResolveGained(MaybeTarget));
				m_ResolvesGainFlat = (MaybeTarget.IsPlayerFaction ? UnitDifficultyMomentumHelper.ResolvesGainedFlatForPartyMemberKill : UnitDifficultyMomentumHelper.GetResolveGainedFlat(MaybeTarget));
				ResultDeltaValueBase = Mathf.FloorToInt((float)Math.Max(m_KillerResolve, 0) * m_ResolvesGain) + m_ResolvesGainFlat;
			}
			break;
		case MomentumChangeReason.StartTurn:
			m_InitiatorResolve = base.ConcreteInitiator.GetStatOptional(StatType.Resolve);
			ResultDeltaValueBase = Math.Max(m_InitiatorResolve, 0);
			break;
		case MomentumChangeReason.AbilityCost:
			ResultDeltaValueBase = -m_Value;
			break;
		case MomentumChangeReason.Custom:
			ResultDeltaValueBase = m_Value;
			break;
		case MomentumChangeReason.Wound:
			ResultDeltaValueBase = -GetResolveLostForWoundOrTrauma(base.ConcreteInitiator, 0.25f);
			break;
		case MomentumChangeReason.Trauma:
		{
			float factor = (base.ConcreteInitiator.IsInPlayerParty ? 0.5f : 0.75f);
			ResultDeltaValueBase = -GetResolveLostForWoundOrTrauma(base.ConcreteInitiator, factor);
			break;
		}
		case MomentumChangeReason.PsychicPhenomena:
			ResultDeltaValueBase = -m_Value;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (group != null)
		{
			if (ResultDeltaValueBase > 0)
			{
				Modifiers.Add(ModifierType.PctAdd, GetDifficultyPercentModifier(group), this, ModifierDescriptor.Difficulty);
			}
			ResultPrevValue = group.Momentum;
			ResultDeltaValue = Modifiers.Apply(ResultDeltaValueBase) + FlatBonus;
			if (ResultPrevValue < 0 && ResultDeltaValue > 0)
			{
				ResultDeltaValue = 0;
			}
			group.AddMomentum(ResultDeltaValue);
			ResultCurrentValue = group.Momentum;
			ResultGroup = group;
		}
	}

	private static int GetDifficultyPercentModifier(MomentumGroup group)
	{
		if (!group.IsParty)
		{
			return 0;
		}
		return SettingsRoot.Difficulty.PartyMomentumPercentModifier;
	}

	private int GetResolveLostForWoundOrTrauma(MechanicEntity unit, float factor = 1f)
	{
		m_Factor = factor;
		m_EntityResolve = unit.GetStatOptional(StatType.Resolve);
		m_ResolveLostBase = (unit.IsPlayerFaction ? UnitDifficultyMomentumHelper.GetResolveLost(UnitDifficultyType.Elite) : UnitDifficultyMomentumHelper.GetResolveLost(unit));
		return Mathf.FloorToInt(m_ResolveLostBase * (float)Math.Max(m_EntityResolve, 0) * m_Factor);
	}
}
