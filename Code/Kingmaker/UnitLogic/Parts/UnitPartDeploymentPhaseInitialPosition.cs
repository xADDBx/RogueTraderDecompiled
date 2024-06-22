using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartDeploymentPhaseInitialPosition : BaseUnitPart, IHashable
{
	[JsonProperty]
	public Vector3 InitialPosition { get; set; }

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Vector3 val2 = InitialPosition;
		result.Append(ref val2);
		return result;
	}
}
