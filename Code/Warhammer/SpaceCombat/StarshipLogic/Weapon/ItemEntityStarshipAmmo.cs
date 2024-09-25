using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Weapon;

public class ItemEntityStarshipAmmo : ItemEntity<BlueprintStarshipAmmo>, IHashable
{
	public ItemEntityStarshipWeapon LoadedIntoWeapon => AmmoSlot?.WeaponSlot?.Weapon;

	public bool IsEquiped => base.HoldingSlot != null;

	public AmmoSlot AmmoSlot => base.HoldingSlot as AmmoSlot;

	public ItemEntityStarshipAmmo([NotNull] BlueprintStarshipAmmo bpItem)
		: base(bpItem)
	{
	}

	[JsonConstructor]
	public ItemEntityStarshipAmmo(JsonConstructorMark _)
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
