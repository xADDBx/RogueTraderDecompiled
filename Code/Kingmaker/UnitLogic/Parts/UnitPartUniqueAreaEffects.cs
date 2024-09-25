using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartUniqueAreaEffects : BaseUnitPart, IAreaHandler, ISubscriber, IHashable
{
	public class AreaEffectListEntry : IHashable
	{
		[JsonProperty]
		[CanBeNull]
		public BlueprintUnitFact Feature;

		[JsonProperty]
		[CanBeNull]
		public string AreaId;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Feature);
			result.Append(ref val);
			result.Append(AreaId);
			return result;
		}
	}

	[JsonProperty]
	public List<AreaEffectListEntry> Areas = new List<AreaEffectListEntry>();

	public void NewAreaEffect(AreaEffectEntity newAreaEffect, BlueprintUnitFact feature)
	{
		List<AreaEffectListEntry> list = new List<AreaEffectListEntry>();
		foreach (AreaEffectListEntry area in Areas)
		{
			AreaEffectEntity entity = EntityService.Instance.GetEntity<AreaEffectEntity>(area.AreaId);
			if (entity == null)
			{
				list.Add(area);
			}
			else if (entity.Blueprint == newAreaEffect.Blueprint || area.Feature == feature)
			{
				list.Add(area);
				entity.ForceEnd();
			}
		}
		foreach (AreaEffectListEntry item2 in list)
		{
			Areas.Remove(item2);
		}
		AreaEffectListEntry item = new AreaEffectListEntry
		{
			Feature = feature,
			AreaId = newAreaEffect.UniqueId
		};
		Areas.Add(item);
	}

	public void RemoveAreaEffect(AreaEffectEntity newAreaEffect, BlueprintUnitFact feature)
	{
		List<AreaEffectListEntry> list = new List<AreaEffectListEntry>();
		foreach (AreaEffectListEntry area in Areas)
		{
			AreaEffectEntity entity = EntityService.Instance.GetEntity<AreaEffectEntity>(area.AreaId);
			if (entity == null)
			{
				list.Add(area);
			}
			else if (entity.Blueprint == newAreaEffect.Blueprint || area.Feature == feature)
			{
				list.Add(area);
				entity.ForceEnd();
			}
		}
		foreach (AreaEffectListEntry item in list)
		{
			Areas.Remove(item);
		}
	}

	public void OnAreaBeginUnloading()
	{
		foreach (AreaEffectListEntry area in Areas)
		{
			EntityService.Instance.GetEntity<AreaEffectEntity>(area.AreaId).ForceEnd();
		}
		Areas.Clear();
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<AreaEffectListEntry> areas = Areas;
		if (areas != null)
		{
			for (int i = 0; i < areas.Count; i++)
			{
				Hash128 val2 = ClassHasher<AreaEffectListEntry>.GetHash128(areas[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
