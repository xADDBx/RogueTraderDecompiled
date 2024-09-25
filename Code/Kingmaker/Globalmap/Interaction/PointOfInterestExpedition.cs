using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestExpedition : BasePointOfInterest, IHashable
{
	private BlueprintStarSystemObject m_Sso;

	public new BlueprintPointOfInterestExpedition Blueprint => (BlueprintPointOfInterestExpedition)base.Blueprint;

	public PointOfInterestExpedition(BlueprintPointOfInterestExpedition blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestExpedition(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void Interact(StarSystemObjectEntity entity)
	{
		m_Sso = entity.Blueprint;
		EventBus.RaiseEvent(delegate(IExplorationExpeditionHandler h)
		{
			h.HandlePointOfInterestExpeditionInteraction(this);
		});
	}

	public void SendExpedition(int peopleCount)
	{
		float num = (float)peopleCount / (float)Blueprint.MaxExpeditionPeopleCount;
		List<LootEntry> list = null;
		for (int i = 0; i < Blueprint.Rewards.Count - 1; i++)
		{
			if ((Mathf.Abs(num - Blueprint.Rewards[i].ExpeditionProportion) <= float.Epsilon || num >= Blueprint.Rewards[i].ExpeditionProportion) && num < Blueprint.Rewards[i + 1].ExpeditionProportion)
			{
				list = Blueprint.Rewards[i].Loot;
				break;
			}
		}
		if (list == null)
		{
			List<BlueprintPointOfInterestExpedition.ExpeditionLoot> rewards = Blueprint.Rewards;
			if (Mathf.Abs(num - rewards[rewards.Count - 1].ExpeditionProportion) <= float.Epsilon)
			{
				List<BlueprintPointOfInterestExpedition.ExpeditionLoot> rewards2 = Blueprint.Rewards;
				list = rewards2[rewards2.Count - 1].Loot;
			}
		}
		if (list == null)
		{
			PFLog.Default.Warning("Something wrong with expedition loot in " + m_Sso.Name);
		}
		else
		{
			Game.Instance.Player.StarSystemsState.SentExpedition.Add(new ExpeditionInfo
			{
				StarSystemObject = m_Sso,
				Loot = list,
				PointOfInterest = Blueprint
			});
		}
		ChangeStatusToInteracted();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
