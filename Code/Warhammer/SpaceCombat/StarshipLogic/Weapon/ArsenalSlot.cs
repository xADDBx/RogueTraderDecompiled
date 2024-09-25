using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Weapon;

public class ArsenalSlot : ItemSlot, IHashable
{
	private Action m_OnItemUpdated;

	public ArsenalSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public ArsenalSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public void Initialize(Action onItemUpdated)
	{
		m_OnItemUpdated = onItemUpdated;
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.IsInCombat)
		{
			return false;
		}
		return item?.Blueprint is BlueprintItemArsenal;
	}

	public override bool CanRemoveItem()
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.IsInCombat)
		{
			return false;
		}
		return base.CanRemoveItem();
	}

	protected override void OnItemInserted()
	{
		base.OnItemInserted();
		m_OnItemUpdated?.Invoke();
	}

	public override bool RemoveItem(bool autoMerge = true, bool force = false)
	{
		bool result = base.RemoveItem(autoMerge, force);
		Action onItemUpdated = m_OnItemUpdated;
		if (onItemUpdated != null)
		{
			onItemUpdated();
			return result;
		}
		return result;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
