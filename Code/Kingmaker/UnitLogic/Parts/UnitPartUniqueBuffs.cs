using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartUniqueBuffs : BaseUnitPart, IAreaHandler, ISubscriber, IHashable
{
	[JsonProperty]
	public List<Buff> Buffs = new List<Buff>();

	public void NewBuff(Buff newBuff)
	{
		Buff buff = Buffs.Find((Buff p) => p.Blueprint == newBuff.Blueprint);
		if (buff != null)
		{
			Buffs.Remove(buff);
			buff.Remove();
		}
		Buffs.Add(newBuff);
	}

	public void RemoveBuff(Buff buff)
	{
		Buff buff2 = Buffs.Find((Buff p) => p.Blueprint == buff.Blueprint);
		if (buff2 != null)
		{
			Buffs.Remove(buff2);
		}
	}

	public void OnAreaBeginUnloading()
	{
		Buffs.RemoveAll(delegate(Buff b)
		{
			if (b.Owner.HoldingState != base.Owner.HoldingState)
			{
				b.Remove();
				return true;
			}
			return false;
		});
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Buff> buffs = Buffs;
		if (buffs != null)
		{
			for (int i = 0; i < buffs.Count; i++)
			{
				Hash128 val2 = ClassHasher<Buff>.GetHash128(buffs[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
