using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.AreaLogic;

public class AreaVailValuePart : EntityPart, IHashable
{
	[JsonProperty]
	public int MinimalVailForCurrentArea;

	[JsonProperty]
	public int Vail;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref MinimalVailForCurrentArea);
		result.Append(ref Vail);
		return result;
	}
}
