using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

public class UsableSlot : ItemSlot, IHashable
{
	[NotNull]
	public new ItemEntityUsable Item => (ItemEntityUsable)base.Item;

	public UsableSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public UsableSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		return item is ItemEntityUsable;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
