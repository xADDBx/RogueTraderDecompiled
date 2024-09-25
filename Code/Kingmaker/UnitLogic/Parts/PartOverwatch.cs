using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartOverwatch : BaseUnitPart, IHashable
{
	[JsonProperty]
	private AbilityData m_Ability;

	[JsonProperty]
	private AbilityData m_AbilityOnTrigger;

	[JsonProperty]
	private TargetWrapper m_Target;

	[JsonProperty]
	private readonly List<EntityFactRef<Buff>> m_GainedBuffs = new List<EntityFactRef<Buff>>();

	[JsonProperty]
	private readonly List<EntityRef<BaseUnitEntity>> m_AttackedUnits = new List<EntityRef<BaseUnitEntity>>();

	private readonly HashSet<CustomGridNodeBase> m_OverwatchArea = new HashSet<CustomGridNodeBase>();

	private bool m_AreaIsConfigured;

	[JsonProperty]
	public OverwatchMode Mode { get; private set; }

	[JsonProperty]
	public OverwatchHitsPerTarget HitsPerTarget { get; private set; }

	public bool IsStopped { get; private set; }

	public AbilityData Ability => m_Ability;

	public AbilityData AbilityOnTrigger => m_AbilityOnTrigger;

	public IReadOnlyCollection<CustomGridNodeBase> OverwatchArea => m_OverwatchArea;

	public void Start([NotNull] AbilityData ability, [NotNull] AbilityData abilityOnTrigger, [NotNull] TargetWrapper target, OverwatchMode mode = OverwatchMode.Overwatch, OverwatchHitsPerTarget hitsPerTarget = OverwatchHitsPerTarget.HitOnce)
	{
		Clear();
		m_Ability = ability ?? throw new ArgumentNullException("ability");
		m_AbilityOnTrigger = abilityOnTrigger ?? throw new ArgumentNullException("abilityOnTrigger");
		m_Target = target ?? throw new ArgumentNullException("target");
		AbilityEffectOverwatch component = ability.Blueprint.GetComponent<AbilityEffectOverwatch>();
		if (component == null)
		{
			PFLog.Default.ErrorWithReport($"AbilityEffectOverwatch is missing: {ability}");
			Stop();
			return;
		}
		if (!TrySetupOverwatchArea())
		{
			PFLog.Default.ErrorWithReport("Overwatch area is empty!");
			Stop();
			return;
		}
		Mode = mode;
		HitsPerTarget = hitsPerTarget;
		BlueprintBuffReference[] applyingBuffs = component.ApplyingBuffs;
		foreach (BlueprintBuffReference blueprintBuffReference in applyingBuffs)
		{
			Buff buff = base.Owner.Buffs.Add(blueprintBuffReference, base.Owner);
			m_GainedBuffs.Add(buff);
		}
	}

	private bool TrySetupOverwatchArea()
	{
		if (m_AreaIsConfigured)
		{
			return true;
		}
		m_AreaIsConfigured = true;
		m_OverwatchArea.AddRange(AbilityEffectOverwatch.GetOverwatchArea(Ability, m_Target));
		return !m_OverwatchArea.Empty();
	}

	private void Clear()
	{
		ClearBuffs();
		m_Ability = null;
		m_AbilityOnTrigger = null;
		Mode = OverwatchMode.Overwatch;
		HitsPerTarget = OverwatchHitsPerTarget.HitOnce;
		m_AttackedUnits.Clear();
		m_OverwatchArea.Clear();
		m_AreaIsConfigured = false;
	}

	private void ClearBuffs()
	{
		foreach (EntityFactRef<Buff> gainedBuff in m_GainedBuffs)
		{
			gainedBuff.Fact?.Remove();
		}
		m_GainedBuffs.Clear();
	}

	public void Stop()
	{
		ClearBuffs();
		IsStopped = true;
		RemoveSelf();
	}

	public bool Contains(BaseUnitEntity unit)
	{
		TrySetupOverwatchArea();
		foreach (CustomGridNodeBase occupiedNode in unit.GetOccupiedNodes())
		{
			if (m_OverwatchArea.Contains(occupiedNode))
			{
				return true;
			}
		}
		return false;
	}

	public void TryTriggerAttack(BaseUnitEntity target)
	{
		if (HitsPerTarget != 0 || !m_AttackedUnits.Contains(target))
		{
			m_AttackedUnits.Add(target);
			UnitOverwatchAttackParams cmdParams = new UnitOverwatchAttackParams(AbilityOnTrigger, target);
			base.Owner.Commands.Run(cmdParams);
			if (Mode == OverwatchMode.Overwatch)
			{
				Stop();
			}
		}
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		m_Ability?.PrePostLoad(base.Owner);
		m_AbilityOnTrigger?.PrePostLoad(base.Owner);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(m_Ability);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<AbilityData>.GetHash128(m_AbilityOnTrigger);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<TargetWrapper>.GetHash128(m_Target);
		result.Append(ref val4);
		List<EntityFactRef<Buff>> gainedBuffs = m_GainedBuffs;
		if (gainedBuffs != null)
		{
			for (int i = 0; i < gainedBuffs.Count; i++)
			{
				EntityFactRef<Buff> obj = gainedBuffs[i];
				Hash128 val5 = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
				result.Append(ref val5);
			}
		}
		List<EntityRef<BaseUnitEntity>> attackedUnits = m_AttackedUnits;
		if (attackedUnits != null)
		{
			for (int j = 0; j < attackedUnits.Count; j++)
			{
				EntityRef<BaseUnitEntity> obj2 = attackedUnits[j];
				Hash128 val6 = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj2);
				result.Append(ref val6);
			}
		}
		OverwatchMode val7 = Mode;
		result.Append(ref val7);
		OverwatchHitsPerTarget val8 = HitsPerTarget;
		result.Append(ref val8);
		return result;
	}
}
