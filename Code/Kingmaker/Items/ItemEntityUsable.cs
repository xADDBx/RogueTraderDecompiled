using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemEntityUsable : ItemEntity<BlueprintItemEquipmentUsable>, IHashable
{
	protected override bool RemoveFromSlotWhenNoCharges => base.Blueprint.RemoveFromSlotWhenNoCharges;

	public ItemEntityUsable([NotNull] BlueprintItemEquipmentUsable bpItem)
		: base(bpItem)
	{
	}

	public ItemEntityUsable(JsonConstructorMark _)
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
