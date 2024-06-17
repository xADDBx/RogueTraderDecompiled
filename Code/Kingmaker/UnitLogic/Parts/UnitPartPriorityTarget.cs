using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartPriorityTarget : BaseUnitPart, IAreaHandler, ISubscriber, IHashable
{
	[JsonProperty]
	private readonly List<EntityFactRef<Buff>> m_PriorityTargets = new List<EntityFactRef<Buff>>();

	public void AddTarget(Buff buff)
	{
		RemoveTarget(buff);
		m_PriorityTargets.Add(buff);
	}

	public void RemoveTarget(Buff buff)
	{
		EntityFactRef<Buff> item = m_PriorityTargets.FirstItem((EntityFactRef<Buff> p) => p.Fact?.Blueprint == buff.Blueprint);
		if (!item.IsEmpty)
		{
			item.Fact?.Remove();
			m_PriorityTargets.Remove(item);
		}
	}

	public BaseUnitEntity GetPriorityTarget(BlueprintBuff buff)
	{
		return m_PriorityTargets.FirstItem((EntityFactRef<Buff> p) => p.Fact?.Blueprint == buff).Fact?.Owner;
	}

	public void OnAreaBeginUnloading()
	{
		foreach (EntityFactRef<Buff> priorityTarget in m_PriorityTargets)
		{
			priorityTarget.Fact?.Remove();
		}
		m_PriorityTargets.Clear();
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<EntityFactRef<Buff>> priorityTargets = m_PriorityTargets;
		if (priorityTargets != null)
		{
			for (int i = 0; i < priorityTargets.Count; i++)
			{
				EntityFactRef<Buff> obj = priorityTargets[i];
				Hash128 val2 = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
