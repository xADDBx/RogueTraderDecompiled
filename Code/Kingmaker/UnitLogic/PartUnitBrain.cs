using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.AI;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.Blueprints;
using Kingmaker.AI.Scenarios;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class PartUnitBrain : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitBrain>, IEntityPartOwner
	{
		PartUnitBrain Brain { get; }
	}

	[JsonProperty]
	private bool m_AiEnabled = true;

	private BehaviourTree m_BehaviourTree;

	[CanBeNull]
	[JsonProperty]
	private AiScenario m_CurrentScenario;

	[JsonProperty]
	private List<EntityRef<MechanicEntity>> m_CustomHatedTargets = new List<EntityRef<MechanicEntity>>();

	[JsonProperty]
	private List<EntityRef<MechanicEntity>> m_CustomLowPriorityTargets = new List<EntityRef<MechanicEntity>>();

	private ScoreOrder m_ForcedOrder;

	public List<EntityRef<MechanicEntity>> CustomLowPriorityTargets => m_CustomLowPriorityTargets;

	[CanBeNull]
	[JsonProperty]
	public BlueprintBrainBase Blueprint { get; private set; }

	[JsonProperty]
	public TimeSpan TurnStartTime { get; private set; }

	[JsonProperty]
	public TimeSpan TurnEndTime { get; private set; }

	[JsonProperty]
	public int IdleRoundsCount { get; set; }

	[JsonProperty]
	public bool IsIdling { get; set; }

	public bool IsTraitor { get; set; }

	public bool IgnoreAoOThreatOnCast { get; set; }

	public bool EnemyConditionsDirty { get; set; }

	public BaseUnitEntity Unit
	{
		get
		{
			if (!(base.Owner is UnitSquad unitSquad))
			{
				return base.Owner as BaseUnitEntity;
			}
			return unitSquad.Leader;
		}
	}

	public AiScenario CurrentScenario
	{
		get
		{
			if (m_CurrentScenario != null && !m_CurrentScenario.IsComplete)
			{
				return m_CurrentScenario;
			}
			return null;
		}
	}

	public bool IsFinishedTurn => m_BehaviourTree.IsFinishedTurn;

	public bool IsAIEnabled
	{
		get
		{
			if (!base.Owner.IsDirectlyControllable)
			{
				return true;
			}
			if (m_AiEnabled)
			{
				return Game.Instance.BlueprintRoot.CompanionsAI;
			}
			return false;
		}
		set
		{
			m_AiEnabled = value;
		}
	}

	public bool IsActingEnabled => Game.Instance.TimeController.RealTime >= (base.Owner.IsInSquad ? TurnStartTime : (TurnStartTime + AiBrainController.TimeToWaitAtStart));

	public ScoreOrder ScoreOrder => m_ForcedOrder ?? (Blueprint as BlueprintBrain)?.ScoreOrder ?? null;

	public AbilitySourceWrapper[] PriorityAbilities => (Blueprint as BlueprintBrain)?.AbilityPriorityOrder.Order ?? null;

	public AbilitySourceWrapper[] MovementInfluentAbilities => (Blueprint as BlueprintBrain)?.MovementInfluentAbilities ?? null;

	public bool IsHoldingPosition => CurrentScenario is HoldPositionScenario;

	public bool IsBreaching => CurrentScenario is BreachScenario;

	public bool IsHuntingDownPriorityTarget => CurrentScenario is PriorityTargetScenario;

	public float HitUnintendedTargetPenalty => (Blueprint as BlueprintBrain)?.HitUnintendedTargetPenalty ?? 1f;

	public bool IsCarefulShooter
	{
		get
		{
			if (!Unit.Blueprint.IsCarefulShooter)
			{
				return (Blueprint as BlueprintBrain)?.IsCarefulShooter ?? false;
			}
			return true;
		}
	}

	public bool ResponseToAoOThreat => (Blueprint as BlueprintBrain)?.ResponseToAoOThreat ?? false;

	public bool ResponseToAoOThreatAfterAbility => (Blueprint as BlueprintBrain)?.ResponseToAoOThreatAfterAbilities ?? false;

	public bool IsUsualMeleeUnit
	{
		get
		{
			ItemEntityWeapon firstWeapon = base.Owner.GetFirstWeapon();
			if (firstWeapon != null && firstWeapon.Blueprint.IsMelee)
			{
				return ((Blueprint as BlueprintBrain)?.MeleeBrainType ?? MeleeBrainType.Usual) == MeleeBrainType.Usual;
			}
			return false;
		}
	}

	public void SetBrain(BlueprintBrainBase brain)
	{
		Blueprint = brain;
		m_BehaviourTree = BehaviourTreeBuilder.Create(base.Owner);
	}

	public void RestoreAvailableActions()
	{
	}

	public void Init()
	{
		m_BehaviourTree.Init();
		IgnoreAoOThreatOnCast = Blueprint is BlueprintBrain blueprintBrain && blueprintBrain.IgnoreAoOForCasting;
		TimeSpan turnEndTime = (TurnStartTime = Game.Instance.TimeController.RealTime);
		TurnEndTime = turnEndTime;
	}

	public void Tick()
	{
		m_BehaviourTree.Tick();
		if (!IsFinishedTurn)
		{
			TurnEndTime = Game.Instance.TimeController.RealTime;
		}
	}

	public List<TargetInfo> GetHatedTargets(List<TargetInfo> targets)
	{
		List<TargetInfo> list = Blueprint?.GetHatedTargets(new PropertyContext(base.Owner, null), targets) ?? TempList.Get<TargetInfo>();
		foreach (EntityRef<MechanicEntity> target in m_CustomHatedTargets)
		{
			TargetInfo targetInfo = targets.FirstOrDefault((TargetInfo t) => t.Entity == target.Entity);
			if (targetInfo != null && !list.Contains(targetInfo))
			{
				list.Add(targetInfo);
			}
		}
		return list;
	}

	public void AddCustomHatedTarget(MechanicEntity target)
	{
		if (!m_CustomHatedTargets.Contains((EntityRef<MechanicEntity> @ref) => @ref.Entity == target))
		{
			m_CustomHatedTargets.Add(target);
			EnemyConditionsDirty = true;
		}
	}

	public void RemoveCustomHatedTarget(MechanicEntity target)
	{
		m_CustomHatedTargets.Remove(target);
		EnemyConditionsDirty = true;
	}

	public void AddCustomLowPriorityTarget(MechanicEntity target)
	{
		if (!m_CustomLowPriorityTargets.Contains((EntityRef<MechanicEntity> @ref) => @ref.Entity == target))
		{
			m_CustomLowPriorityTargets.Add(target);
			EnemyConditionsDirty = true;
		}
	}

	public void RemoveCustomLowPriorityTarget(MechanicEntity target)
	{
		m_CustomLowPriorityTargets.Remove(target);
		EnemyConditionsDirty = true;
	}

	public void ClearCustomLowPriorityTargets()
	{
		m_CustomLowPriorityTargets.Clear();
		EnemyConditionsDirty = true;
	}

	public float GetTargetPriority(AbilityData ability, MechanicEntity target)
	{
		BlueprintBrainBase blueprint = Blueprint;
		if (blueprint != null && blueprint.GetPriorityDestroyTarget().Contains(target))
		{
			return 1f;
		}
		AbilitySettings abilitySettings = Blueprint?.GetCustomAbilitySettings(ability.Blueprint);
		if (abilitySettings == null)
		{
			return 0f;
		}
		PropertyContext context = new PropertyContext(base.Owner, null, target);
		if (abilitySettings.IsHighPriority(context))
		{
			return 0.9f;
		}
		if (abilitySettings.IsLowPriority(context))
		{
			return -1f;
		}
		return 0f;
	}

	public int GetAbilityValue(AbilityData ability, MechanicEntity target)
	{
		if (!(base.Owner is StarshipEntity))
		{
			return 0;
		}
		PropertyContext context = new PropertyContext(base.Owner, null, target);
		return Blueprint?.GetAbilityValue(ability.Blueprint, context) ?? 1;
	}

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		SetBrain(Unit?.Blueprint.DefaultBrain);
		RestoreAvailableActions();
	}

	public void UpdateIdleRoundsCounter()
	{
		IdleRoundsCount = (IsIdling ? (IdleRoundsCount + 1) : 0);
		IsIdling = true;
	}

	public void SetHoldPosition(TargetWrapper target, int range, int idleRoundsCount, bool completeOnTargetReached)
	{
		m_CurrentScenario = new HoldPositionScenario(Unit, target, range, idleRoundsCount, completeOnTargetReached);
		EventBus.RaiseEvent((IBaseUnitEntity)Unit, (Action<IUnitAiScenarioHandler>)delegate(IUnitAiScenarioHandler h)
		{
			h.HandleScenarioActivated(m_CurrentScenario);
		}, isCheckRuntime: true);
	}

	public void SetPriorityTarget(IEnumerable<MechanicEntityEvaluator> targets, bool simultaneousTargets, bool attackPriorityTargetsOnly, int idleRoundsCount)
	{
		m_CurrentScenario = new PriorityTargetScenario(Unit, targets, simultaneousTargets, attackPriorityTargetsOnly, idleRoundsCount);
		EventBus.RaiseEvent((IBaseUnitEntity)Unit, (Action<IUnitAiScenarioHandler>)delegate(IUnitAiScenarioHandler h)
		{
			h.HandleScenarioActivated(m_CurrentScenario);
		}, isCheckRuntime: true);
	}

	public void ForceScorePairPriority(ScorePair pair)
	{
		m_ForcedOrder = ScoreOrder.WithForcedPriority(pair);
	}

	public void ResetScorePairPriority()
	{
		m_ForcedOrder = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_AiEnabled);
		Hash128 val2 = ClassHasher<AiScenario>.GetHash128(m_CurrentScenario);
		result.Append(ref val2);
		List<EntityRef<MechanicEntity>> customHatedTargets = m_CustomHatedTargets;
		if (customHatedTargets != null)
		{
			for (int i = 0; i < customHatedTargets.Count; i++)
			{
				EntityRef<MechanicEntity> obj = customHatedTargets[i];
				Hash128 val3 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
				result.Append(ref val3);
			}
		}
		List<EntityRef<MechanicEntity>> customLowPriorityTargets = m_CustomLowPriorityTargets;
		if (customLowPriorityTargets != null)
		{
			for (int j = 0; j < customLowPriorityTargets.Count; j++)
			{
				EntityRef<MechanicEntity> obj2 = customLowPriorityTargets[j];
				Hash128 val4 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj2);
				result.Append(ref val4);
			}
		}
		Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val5);
		TimeSpan val6 = TurnStartTime;
		result.Append(ref val6);
		TimeSpan val7 = TurnEndTime;
		result.Append(ref val7);
		int val8 = IdleRoundsCount;
		result.Append(ref val8);
		bool val9 = IsIdling;
		result.Append(ref val9);
		return result;
	}
}
