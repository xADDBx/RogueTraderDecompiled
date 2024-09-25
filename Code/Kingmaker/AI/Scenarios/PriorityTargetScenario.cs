using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Scenarios;

public class PriorityTargetScenario : AiScenario, IHashable
{
	[JsonProperty]
	public readonly List<MechanicEntityEvaluator> Targets = new List<MechanicEntityEvaluator>();

	[JsonProperty]
	public readonly bool SimultaneousTargets;

	[JsonProperty]
	public readonly bool AttackPriorityTargetsOnly;

	public TargetInfo CurrentTarget()
	{
		TryClearDestroyedTargets();
		if (Targets.Count > 0)
		{
			TargetInfo targetInfo = new TargetInfo();
			targetInfo.Init(Targets[0].GetValue());
			return targetInfo;
		}
		return null;
	}

	public IEnumerable<TargetInfo> AllNotDestroyedTarget()
	{
		TryClearDestroyedTargets();
		List<TargetInfo> list = new List<TargetInfo>();
		foreach (MechanicEntityEvaluator target in Targets)
		{
			_ = target;
			TargetInfo targetInfo = new TargetInfo();
			targetInfo.Init(Targets[0].GetValue());
			list.Add(targetInfo);
		}
		return list;
	}

	public PriorityTargetScenario(BaseUnitEntity owner, IEnumerable<MechanicEntityEvaluator> targets, bool simultaneousTargets, bool attackPriorityTargetsOnly, int idleRoundsCountLimit)
		: base(owner, idleRoundsCountLimit)
	{
		if (targets != null)
		{
			Targets.AddRange(targets);
			SimultaneousTargets = simultaneousTargets;
			AttackPriorityTargetsOnly = attackPriorityTargetsOnly;
		}
	}

	public override bool ShouldComplete()
	{
		return !Targets.HasItem((MechanicEntityEvaluator target) => !IsTargetDestroyed(target.GetValue()));
	}

	private bool IsTargetDestroyed(MechanicEntity entity)
	{
		if (entity != null && entity.IsDeadOrUnconscious)
		{
			return true;
		}
		PartHealth partHealth = entity?.Parts.GetOptional<PartHealth>();
		if (partHealth != null && partHealth.HitPointsLeft <= 0)
		{
			return true;
		}
		return false;
	}

	private void TryClearDestroyedTargets()
	{
		List<MechanicEntityEvaluator> list = Targets.ToList();
		foreach (MechanicEntityEvaluator target in Targets)
		{
			if (IsTargetDestroyed(target.GetValue()))
			{
				list.Remove(target);
			}
		}
		Targets.Clear();
		Targets.AddRange(list);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<MechanicEntityEvaluator> targets = Targets;
		if (targets != null)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				Hash128 val2 = MechanicEntityEvaluatorHasher.GetHash128(targets[i]);
				result.Append(ref val2);
			}
		}
		bool val3 = SimultaneousTargets;
		result.Append(ref val3);
		bool val4 = AttackPriorityTargetsOnly;
		result.Append(ref val4);
		return result;
	}
}
