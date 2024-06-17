using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformSummonUnit : RulebookEvent
{
	private List<BlueprintUnitFact> m_AddFacts;

	private HashSet<BlueprintBuff> m_AddBuffs;

	public readonly BlueprintUnit Blueprint;

	public readonly int Level;

	public readonly Vector3 Position;

	public readonly Rounds Duration;

	public BaseUnitEntity SummonedUnit { get; private set; }

	public MechanicsContext Context { get; set; }

	public bool DoNotLinkToCaster { get; set; }

	public Rounds BonusDuration { get; set; }

	public RulePerformSummonUnit([NotNull] MechanicEntity initiator, [NotNull] BlueprintUnit blueprint, Vector3 position, Rounds duration, int level)
		: base(initiator)
	{
		Blueprint = blueprint;
		Position = position;
		Duration = duration;
		Level = level;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		bool flag = Context is AbilityExecutionContext abilityExecutionContext && abilityExecutionContext.ExecutionFromPsychicPhenomena;
		BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(Blueprint, Position, Quaternion.identity, (!flag) ? base.ConcreteInitiator.HoldingState : null);
		if (baseUnitEntity == null)
		{
			return;
		}
		if (!DoNotLinkToCaster)
		{
			PartFaction factionOptional = base.ConcreteInitiator.GetFactionOptional();
			if (factionOptional != null)
			{
				baseUnitEntity.Faction.Set(factionOptional.Blueprint);
			}
			PartCombatGroup combatGroupOptional = base.ConcreteInitiator.GetCombatGroupOptional();
			if (combatGroupOptional != null)
			{
				baseUnitEntity.CombatGroup.Id = combatGroupOptional.Id;
			}
		}
		baseUnitEntity.GetOrCreate<UnitPartSummonedMonster>().Init(DoNotLinkToCaster ? baseUnitEntity : base.ConcreteInitiator);
		Rounds value = Duration + BonusDuration;
		if (Context != null)
		{
			baseUnitEntity.Buffs.Add(Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff, Context, value);
		}
		else
		{
			baseUnitEntity.Buffs.Add(Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff, base.ConcreteInitiator, value);
		}
		SummonedUnit = baseUnitEntity;
		if (m_AddFacts != null)
		{
			foreach (BlueprintUnitFact addFact in m_AddFacts)
			{
				SummonedUnit.AddFact(addFact);
			}
		}
		if (m_AddBuffs != null)
		{
			foreach (BlueprintBuff addBuff in m_AddBuffs)
			{
				SummonedUnit.Buffs.Add(addBuff, base.ConcreteInitiator, Context);
			}
		}
		SummonedUnit.Buffs.Add(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff, Context, 1.Rounds());
		MechanicsContext context2 = Context;
		if (context2 != null && context2.IsShadow)
		{
			baseUnitEntity.GetOrCreate<UnitPartShadowSummon>().Setup(Context);
		}
		if (SettingsRoot.Game.TurnBased.EnableTurnBasedMode && SummonedUnit.Blueprint != BlueprintRoot.Instance.SystemMechanics.LedgermainUnit)
		{
			SummonedUnit.Facts.Remove(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff);
		}
	}

	public void AddFact(BlueprintUnitFact fact)
	{
		m_AddFacts = m_AddFacts ?? new List<BlueprintUnitFact>();
		m_AddFacts.Add(fact);
	}

	public void AddBuff(BlueprintBuff buff, TimeSpan? duration = null)
	{
		m_AddBuffs = m_AddBuffs ?? new HashSet<BlueprintBuff>();
		m_AddBuffs.Add(buff);
	}
}
