using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("5907f3b54e2e4718a1f00356e189a937")]
public class BlockOverpenetration : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.BlockOverpenetration.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.BlockOverpenetration.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
