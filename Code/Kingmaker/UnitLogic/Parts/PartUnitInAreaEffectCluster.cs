using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartUnitInAreaEffectCluster : BaseUnitPart, IHashable
{
	private readonly HashSet<BlueprintAbilityAreaEffectClusterLogic> m_ClusterKeys = new HashSet<BlueprintAbilityAreaEffectClusterLogic>();

	private readonly Dictionary<BlueprintAbilityAreaEffectClusterLogic, List<AreaEffectEntity>> m_AreaEffectEntitiesInVisit = new Dictionary<BlueprintAbilityAreaEffectClusterLogic, List<AreaEffectEntity>>();

	public HashSet<BlueprintAbilityAreaEffectClusterLogic> ClusterKeys => m_ClusterKeys;

	public Dictionary<BlueprintAbilityAreaEffectClusterLogic, List<AreaEffectEntity>> AreaEffectEntitiesInVisit => m_AreaEffectEntitiesInVisit;

	public void AddClusterKey(BlueprintAbilityAreaEffectClusterLogic blueprint)
	{
		m_ClusterKeys.Add(blueprint);
	}

	public void RemoveClusterKey(BlueprintAbilityAreaEffectClusterLogic blueprint)
	{
		m_ClusterKeys.Remove(blueprint);
		m_AreaEffectEntitiesInVisit.Remove(blueprint);
		RemoveSelfIfEmpty();
	}

	public void AddEnteringAreaEffectToList(BlueprintAbilityAreaEffectClusterLogic blueprint, AreaEffectEntity entity)
	{
		if (m_AreaEffectEntitiesInVisit.ContainsKey(blueprint))
		{
			m_AreaEffectEntitiesInVisit[blueprint].Add(entity);
			return;
		}
		m_AreaEffectEntitiesInVisit[blueprint] = new List<AreaEffectEntity> { entity };
	}

	public void RemoveExitingAreaEffectFromList(BlueprintAbilityAreaEffectClusterLogic blueprint, AreaEffectEntity entity)
	{
		m_AreaEffectEntitiesInVisit[blueprint].Remove(entity);
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_ClusterKeys.Empty())
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
