using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

public class Contract : Quest, IHashable
{
	[JsonProperty]
	public Dictionary<BlueprintResource, int> ResourceShortage = new Dictionary<BlueprintResource, int>();

	public Contract(BlueprintQuest blueprintQuest)
		: base(blueprintQuest)
	{
	}

	protected Contract(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnQuestFinished(bool completed)
	{
		base.OnQuestFinished(completed);
		foreach (Requirement component in base.Blueprint.GetComponents<Requirement>())
		{
			component.Apply();
		}
		foreach (Reward component2 in base.Blueprint.GetComponents<Reward>())
		{
			component2.ReceiveReward();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintResource, int> resourceShortage = ResourceShortage;
		if (resourceShortage != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item in resourceShortage)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				int obj = item.Value;
				Hash128 val4 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		return result;
	}
}
