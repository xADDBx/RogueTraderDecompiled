using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("e3329f4993e5499383fd9b5291466a43")]
public class WarhammerChosenWeaponRemover : UnitBuffComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		WarhammerUnitPartChooseWeapon orCreate = base.Owner.GetOrCreate<WarhammerUnitPartChooseWeapon>();
		ItemEntityWeapon maybeWeapon = base.Owner.Body.PrimaryHand.MaybeWeapon;
		if (maybeWeapon != null)
		{
			orCreate.ChooseWeapon(maybeWeapon);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<WarhammerUnitPartChooseWeapon>();
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
