using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public class AbilityExecutionContext : MechanicsContext, IHashable
{
	public new class Data : ContextData<Data>
	{
		public RulePerformAttack AttackRule { get; private set; }

		public Projectile Projectile { get; private set; }

		public TargetWrapper ClickedTarget { get; private set; }

		public Data Setup([NotNull] RulePerformAttack rule, [NotNull] Projectile projectile, TargetWrapper clickedTarget)
		{
			AttackRule = rule;
			Projectile = projectile;
			ClickedTarget = clickedTarget;
			return this;
		}

		protected override void Reset()
		{
			AttackRule = null;
			Projectile = null;
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private readonly EntityFactRef<Ability> m_AbilityFact;

	[JsonProperty]
	private readonly AbilityData m_Ability;

	[JsonProperty]
	public readonly TargetWrapper ClickedTarget;

	[JsonIgnore]
	private HashSet<EntityRef<UnitEntity>> m_UnitsBlockingAttack;

	private bool m_PatternConfigured;

	[CanBeNull]
	private OrientedPatternData m_Pattern;

	public bool RedirectActive;

	public readonly List<MechanicEntity> RedirectTargets = new List<MechanicEntity>();

	private readonly Vector3 m_CastPosition;

	private Dictionary<CustomGridNodeBase, (WarhammerSingleNodeBlocker Blocker, IntRect Rect, Vector3 Direction)> m_BlockedNodes = new Dictionary<CustomGridNodeBase, (WarhammerSingleNodeBlocker, IntRect, Vector3)>();

	[JsonIgnore]
	public bool ExecutionFromPsychicPhenomena;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public List<TargetWrapper> AllTargets { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int ActionIndex { get; private set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public TimeSpan? DelayBetweenActions { get; private set; }

	[JsonProperty]
	public TimeSpan CastTime { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public AttackHitPolicyType HitPolicy { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public DamagePolicyType DamagePolicy { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool KillTarget { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool DisableLog { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool IsForced { get; set; }

	[CanBeNull]
	public List<AbilityApproachingEffect> ApproachingEffects { get; private set; }

	public IEnumerable<AbilitySpawnFx> FxSpawners => AbilityBlueprint.GetComponents<AbilitySpawnFx>();

	public AbilityData Ability
	{
		get
		{
			if (m_Ability.Caster == null)
			{
				Ability fact = m_AbilityFact.Fact;
				if (fact != null)
				{
					m_Ability.PrePostLoad(fact);
				}
				else if (base.MaybeCaster != null)
				{
					m_Ability.PrePostLoad(base.MaybeCaster);
				}
			}
			return m_Ability;
		}
	}

	[CanBeNull]
	public OrientedPatternData Pattern
	{
		get
		{
			TryConfigurePattern();
			return m_Pattern;
		}
	}

	public BlueprintAbility AbilityBlueprint => Ability.Blueprint;

	public override TargetWrapper MainTarget => ClickedTarget;

	[NotNull]
	public MechanicEntity Caster => base.MaybeCaster ?? throw new Exception("Caster is missing");

	public void TemporarilyBlockNode(Vector3 pos, BaseUnitEntity unit)
	{
		TemporarilyBlockNode(pos.GetNearestNodeXZ(), unit);
	}

	public void TemporarilyBlockLastPathNode(Path path, BaseUnitEntity unit)
	{
		TemporarilyBlockNode(path.vectorPath.Last(), unit);
	}

	public void TemporarilyBlockNode(CustomGridNodeBase node, BaseUnitEntity unit)
	{
		if (!m_BlockedNodes.ContainsKey(node))
		{
			BlockType blockType = (unit.IsInvisible ? BlockType.Invisible : (unit.Faction.IsPlayerEnemy ? BlockType.Enemy : BlockType.Friend));
			WarhammerBlockManager.Instance.InternalBlock(node, unit.MovementAgent.Blocker, unit.SizeRect, unit.Forward, blockType);
			m_BlockedNodes.Add(node, (unit.MovementAgent.Blocker, unit.SizeRect, unit.Forward));
		}
	}

	public void ClearBlockedNodes()
	{
		foreach (KeyValuePair<CustomGridNodeBase, (WarhammerSingleNodeBlocker, IntRect, Vector3)> blockedNode in m_BlockedNodes)
		{
			WarhammerBlockManager.Instance.InternalUnblock(blockedNode.Key, blockedNode.Value.Item1, blockedNode.Value.Item2, blockedNode.Value.Item3);
		}
	}

	public AbilityExecutionContext(AbilityData ability, TargetWrapper clickedTarget, Vector3 casterPosition)
		: base(ability.Caster, null, ability.Blueprint)
	{
		if (ability.Blueprint.HasVariants)
		{
			throw new Exception("Can't cast variational ability");
		}
		m_CastPosition = casterPosition;
		m_AbilityFact = ability.Fact;
		m_Ability = ability;
		ClickedTarget = clickedTarget;
		base.SpellLevel = Ability.SpellLevel;
		base.Direction = (ClickedTarget.IsOrientationSpecified ? (Quaternion.Euler(0f, ClickedTarget.Orientation, 0f) * Vector3.forward).normalized : (ClickedTarget.Point - casterPosition).normalized);
	}

	[JsonConstructor]
	protected AbilityExecutionContext(JsonConstructorMark _)
		: base(_)
	{
	}

	public static Data GetAbilityDataScope(RulePerformAttack rule, Projectile projectile, TargetWrapper clickedTarget)
	{
		return ContextData<Data>.Request().Setup(rule, projectile, clickedTarget);
	}

	private void TryConfigurePattern()
	{
		if (!m_PatternConfigured)
		{
			m_Pattern = Ability.GetPattern(ClickedTarget, m_CastPosition);
			m_PatternConfigured = true;
		}
	}

	public void AddApproachingEffect([NotNull] AbilityApproachingEffect effect)
	{
		if (ApproachingEffects == null)
		{
			List<AbilityApproachingEffect> list2 = (ApproachingEffects = new List<AbilityApproachingEffect>());
		}
		ApproachingEffects.Add(effect);
	}

	public void CleanupApproachingEffects()
	{
		if (ApproachingEffects != null)
		{
			ApproachingEffects.RemoveAll((AbilityApproachingEffect e) => e.FinishedApproaching);
			if (ApproachingEffects.Count <= 0)
			{
				ApproachingEffects = null;
			}
		}
	}

	public void AddUnitBlockingAttack(UnitEntity unit)
	{
		if (unit != null)
		{
			if (m_UnitsBlockingAttack == null)
			{
				m_UnitsBlockingAttack = new HashSet<EntityRef<UnitEntity>>();
			}
			m_UnitsBlockingAttack.Add(new EntityRef<UnitEntity>(unit));
		}
	}

	public bool IsUnitBlockingAttack(UnitEntity unit)
	{
		return m_UnitsBlockingAttack?.Contains(unit) ?? false;
	}

	public override T TriggerRule<T>(T rule)
	{
		rule.Reason = this;
		return base.TriggerRule(rule);
	}

	public void NextAction()
	{
		ActionIndex++;
	}

	public void RewindActionIndex(TimeSpan? delay = null)
	{
		if (delay.HasValue && delay.GetValueOrDefault().TotalSeconds > 0.001)
		{
			DelayBetweenActions = delay;
		}
		else
		{
			ActionIndex = Ability.ActionsCount;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityFactRef<Ability> obj = m_AbilityFact;
		Hash128 val2 = StructHasher<EntityFactRef<Kingmaker.UnitLogic.Abilities.Ability>>.GetHash128(ref obj);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<AbilityData>.GetHash128(m_Ability);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<TargetWrapper>.GetHash128(ClickedTarget);
		result.Append(ref val4);
		List<TargetWrapper> allTargets = AllTargets;
		if (allTargets != null)
		{
			for (int i = 0; i < allTargets.Count; i++)
			{
				Hash128 val5 = ClassHasher<TargetWrapper>.GetHash128(allTargets[i]);
				result.Append(ref val5);
			}
		}
		int val6 = ActionIndex;
		result.Append(ref val6);
		if (DelayBetweenActions.HasValue)
		{
			TimeSpan val7 = DelayBetweenActions.Value;
			result.Append(ref val7);
		}
		TimeSpan val8 = CastTime;
		result.Append(ref val8);
		AttackHitPolicyType val9 = HitPolicy;
		result.Append(ref val9);
		DamagePolicyType val10 = DamagePolicy;
		result.Append(ref val10);
		bool val11 = KillTarget;
		result.Append(ref val11);
		bool val12 = DisableLog;
		result.Append(ref val12);
		bool val13 = IsForced;
		result.Append(ref val13);
		return result;
	}
}
