using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class MeltaChargeRestrictionPart : NeedItemRestrictionPart<MeltaChargeRestrictionSettings>, IHashable
{
	public override bool ShouldCheckSourceComponent => false;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
