using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class PartUnitInvisible : BaseUnitPart, IHashable
{
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
		return result;
	}
}
