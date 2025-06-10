using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemEntityShield : ItemEntity<BlueprintItemShield>, IHashable
{
	[JsonProperty]
	[CanBeNull]
	public ItemEntityWeapon WeaponComponent { get; private set; }

	public ItemEntityShield([NotNull] BlueprintItemShield bpItem)
		: base(bpItem)
	{
		if (bpItem.WeaponComponent != null)
		{
			WeaponComponent = Entity.Initialize(new ItemEntityWeapon(bpItem.WeaponComponent, this));
		}
	}

	protected ItemEntityShield(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void OnDidEquipped(MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		WeaponComponent?.OnDidEquipped(wielder);
		if (WeaponComponent != null)
		{
			WeaponComponent.HoldingSlot = base.HoldingSlot;
		}
	}

	public override void OnWillUnequip()
	{
		WeaponComponent?.OnWillUnequip();
		if (WeaponComponent != null)
		{
			WeaponComponent.HoldingSlot = null;
		}
		base.OnWillUnequip();
	}

	protected override void OnSubscribe()
	{
		base.OnSubscribe();
		WeaponComponent?.Subscribe();
	}

	protected override void OnUnsubscribe()
	{
		base.OnUnsubscribe();
		WeaponComponent?.Unsubscribe();
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		WeaponComponent?.PreSave();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		base.EnchantmentsCollection?.Dispose();
		WeaponComponent?.PostLoad(this);
		bool flag = base.Wielder != null;
		if (WeaponComponent?.Blueprint != base.Blueprint.WeaponComponent)
		{
			ItemEntityWeapon weaponComponent = WeaponComponent;
			WeaponComponent = ((base.Blueprint.WeaponComponent != null) ? Entity.Initialize(new ItemEntityWeapon(base.Blueprint.WeaponComponent, this)) : null);
			if (base.IsIdentified)
			{
				WeaponComponent?.Identify();
			}
			if (flag)
			{
				weaponComponent?.OnWillUnequip();
				WeaponComponent?.OnDidEquipped(base.Wielder);
			}
			PFLog.Default.Warning(string.Format("Replaced WeaponComponent in shield {0}: {1} --> {2}", base.Blueprint, weaponComponent?.Blueprint.ToString() ?? "<null>", WeaponComponent?.Blueprint.ToString() ?? "<null>"));
			weaponComponent?.Dispose();
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		WeaponComponent?.Dispose();
	}

	protected override void OnReapplyFactsForWielder()
	{
		base.OnReapplyFactsForWielder();
		ReapplyAbilitiesImpl();
	}

	private void ReapplyAbilitiesImpl()
	{
		base.Abilities.ForEach(delegate(Ability v)
		{
			base.Wielder.Facts.Remove(v);
		});
		base.Abilities.Clear();
		MechanicEntity wielderUnit = base.Wielder;
		if (base.Blueprint == null || wielderUnit == null)
		{
			return;
		}
		base.Abilities.AddRange(base.Blueprint.WeaponAbilities.AllWithIndex.Where(((int Index, WeaponAbility Slot) i) => i.Slot.Ability != null).Select(delegate((int Index, WeaponAbility Slot) i)
		{
			Ability ability = base.Wielder.Facts.Add(new Ability(i.Slot.Ability, wielderUnit));
			if (ability != null)
			{
				ability.Data.ItemSlotIndex = i.Index;
			}
			ability?.AddSource(this);
			return ability;
		}).NotNull());
		WeaponComponent?.ReapplyAbilities();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemEntityWeapon>.GetHash128(WeaponComponent);
		result.Append(ref val2);
		return result;
	}
}
