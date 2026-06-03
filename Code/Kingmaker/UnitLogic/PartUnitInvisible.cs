using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class PartUnitInvisible : BaseUnitPart, IHashable
{
	[JsonProperty]
	public bool UseAttackOfOpportunity { get; set; }

	protected override void OnAttach()
	{
		base.Owner.UpdateVisible();
	}

	protected override void OnDetach()
	{
		base.Owner.UpdateVisible();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = UseAttackOfOpportunity;
		result.Append(ref val2);
		return result;
	}
}
