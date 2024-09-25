using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Equipment;

public class ItemEntityVoidShieldGenerator : StarshipItemEntity<BlueprintItemVoidShieldGenerator>, IHashable
{
	public ItemEntityVoidShieldGenerator(BlueprintItemVoidShieldGenerator bpItem)
		: base(bpItem)
	{
	}

	protected ItemEntityVoidShieldGenerator(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void OnDidEquipped([NotNull] MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		base.Owner.GetStarshipShieldsOptional()?.ActivateShields();
	}

	public override void OnWillUnequip()
	{
		base.Owner.GetStarshipShieldsOptional()?.DeactivateShields(onUnequip: true);
		base.OnWillUnequip();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
