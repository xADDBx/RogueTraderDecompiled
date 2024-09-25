using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class WarhammerUnitPartBuffLimit : BaseUnitPart, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[JsonProperty]
	public HashSet<BlueprintBuff> WatchedBuffs = new HashSet<BlueprintBuff>();

	public void AddWatchedBuff(BlueprintBuff buffBlueprint)
	{
		WatchedBuffs.Add(buffBlueprint);
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (buff.Context?.MaybeCaster == base.Owner && WatchedBuffs.Contains(buff.Blueprint))
		{
			((buff.Context?.MaybeOwner)?.Buffs.RawFacts.FirstOrDefault((Buff x) => buff.Blueprint == x.Blueprint && buff != x))?.Remove();
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		HashSet<BlueprintBuff> watchedBuffs = WatchedBuffs;
		if (watchedBuffs != null)
		{
			int num = 0;
			foreach (BlueprintBuff item in watchedBuffs)
			{
				num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}
}
