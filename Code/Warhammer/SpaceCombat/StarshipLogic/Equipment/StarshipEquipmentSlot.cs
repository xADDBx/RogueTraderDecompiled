using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items.Slots;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Equipment;

public class StarshipEquipmentSlot<T> : EquipmentSlot<T>, IHashable where T : BlueprintStarshipItem
{
	public StarshipEquipmentSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public StarshipEquipmentSlot(JsonConstructorMark _)
		: base(_)
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
