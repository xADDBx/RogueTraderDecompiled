using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

public class StarshipUnitPartLaunchBayLogic : BaseUnitPart, IHashable
{
	[JsonProperty]
	public int[] cooldowns;

	[JsonProperty]
	public int launchedThisTurn;

	[JsonProperty]
	public int launchedTotal;

	[JsonProperty]
	public int reloadLeft;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(cooldowns);
		result.Append(ref launchedThisTurn);
		result.Append(ref launchedTotal);
		result.Append(ref reloadLeft);
		return result;
	}
}
