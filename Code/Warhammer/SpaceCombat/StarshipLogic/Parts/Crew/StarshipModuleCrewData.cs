using System;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Parts.Crew;

[Serializable]
public class StarshipModuleCrewData : IHashable
{
	[JsonProperty]
	public int CountLost { get; set; }

	[JsonProperty]
	public int CountLostOnCurrentTurn { get; set; }

	[JsonProperty]
	public int CountInTransitionToModule { get; set; }

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		int val = CountLost;
		result.Append(ref val);
		int val2 = CountLostOnCurrentTurn;
		result.Append(ref val2);
		int val3 = CountInTransitionToModule;
		result.Append(ref val3);
		return result;
	}
}
