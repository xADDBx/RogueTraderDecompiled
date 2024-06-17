using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("2d4eed7d55ad42b5b1640ea3d3b26492")]
public class WarhammerBrainBetrayAllies : EntityFactComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		PartUnitBrain optional = base.Owner.GetOptional<PartUnitBrain>();
		if (optional != null)
		{
			optional.IsTraitor = true;
		}
	}

	protected override void OnDeactivate()
	{
		PartUnitBrain optional = base.Owner.GetOptional<PartUnitBrain>();
		if (optional != null)
		{
			optional.IsTraitor = false;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
