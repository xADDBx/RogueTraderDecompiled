using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class StarShipUnitPartDamageCounter : BaseUnitPart, IHashable
{
	[JsonProperty]
	private readonly List<int> DamageAtRoundStart = new List<int>();

	public int RoundsCounted => DamageAtRoundStart.Count();

	public void NewCombat()
	{
		DamageAtRoundStart.Clear();
	}

	public void NewRound()
	{
		DamageAtRoundStart.Add(base.Owner.Health.Damage);
	}

	public int GetDamageFromStart()
	{
		return base.Owner.Health.Damage - DamageAtRoundStart.FirstOrDefault();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<int> damageAtRoundStart = DamageAtRoundStart;
		if (damageAtRoundStart != null)
		{
			for (int i = 0; i < damageAtRoundStart.Count; i++)
			{
				int obj = damageAtRoundStart[i];
				Hash128 val2 = UnmanagedHasher<int>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
