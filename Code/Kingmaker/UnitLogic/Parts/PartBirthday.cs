using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartBirthday : EntityPart, IHashable
{
	[JsonProperty]
	public int Day { get; private set; }

	[JsonProperty]
	public int Month { get; private set; }

	public void Set(int day, int month)
	{
		Day = day;
		Month = month;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = Day;
		result.Append(ref val2);
		int val3 = Month;
		result.Append(ref val3);
		return result;
	}
}
