using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Assets.Code.Designers.Mechanics.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Weapon;

public class ItemEntityStarshipWeapon : StarshipItemEntity<BlueprintStarshipWeapon>, IHashable
{
	[JsonProperty]
	public CountableFlag IsBlocked = new CountableFlag();

	[JsonProperty]
	public ItemEntityStarshipAmmo FakeAmmo { get; set; }

	public StarshipEntity Starship => (StarshipEntity)base.HoldingSlot.Owner;

	public WeaponSlot WeaponSlot => base.HoldingSlot as WeaponSlot;

	private AmmoSlot AmmoSlot => WeaponSlot?.AmmoSlot;

	protected override bool RemoveFromSlotWhenNoCharges => base.Blueprint.RemoveFromSlotWhenNoCharges;

	public ItemEntityStarshipAmmo Ammo
	{
		get
		{
			if (FakeAmmo != null)
			{
				return FakeAmmo;
			}
			return AmmoSlot?.MaybeItem as ItemEntityStarshipAmmo;
		}
	}

	public DamageType DamageType => Ammo?.Blueprint.DamageType.Type ?? DamageType.Direct;

	public bool IsFocusedEnergyWeapon => base.Blueprint.WeaponType == StarshipWeaponType.Lances;

	public bool IsAEAmmo => Ammo.Blueprint.IsAE;

	public ItemEntityStarshipWeapon([NotNull] BlueprintStarshipWeapon bpItem)
		: base(bpItem)
	{
		base.Charges = 0;
	}

	public ItemEntityStarshipWeapon(JsonConstructorMark _)
		: base(_)
	{
	}

	public void Reload()
	{
		if (base.Blueprint != null && Starship.Facts.GetComponents((StarshipBlockRecharge block) => block.Match(this)).Empty())
		{
			int num = (from x in Starship.Facts.GetComponents<StarshipModifyMaxCharges>()
				where x.WeaponType == base.Blueprint.WeaponType
				select x.Value).DefaultIfEmpty(0).Sum();
			base.Charges = base.Blueprint.Charges + num;
		}
	}

	public bool CanEquipAmmo(ItemEntityStarshipAmmo ammo)
	{
		return WeaponSlot.CanEquipAmmo(ammo);
	}

	public void EquipAmmo(ItemEntityStarshipAmmo ammo, bool reloadInstantly = false)
	{
		WeaponSlot.EquipAmmo(ammo, reloadInstantly);
	}

	public void UnequipAmmo()
	{
		WeaponSlot.UnequipAmmo();
	}

	public DamagePredictionData CalculateDamageForTarget(StarshipEntity target)
	{
		RuleStarshipCalculateDamageForTarget ruleStarshipCalculateDamageForTarget = Rulebook.Trigger(new RuleStarshipCalculateDamageForTarget(Starship, target, this));
		return new DamagePredictionData
		{
			MinDamage = ruleStarshipCalculateDamageForTarget.ResultMinDamage * base.Blueprint.DamageInstances,
			MaxDamage = ruleStarshipCalculateDamageForTarget.ResultMaxDamage * base.Blueprint.DamageInstances,
			Penetration = ruleStarshipCalculateDamageForTarget.ResultDamage.Penetration.Value
		};
	}

	protected override void OnReapplyFactsForWielder()
	{
		base.OnReapplyFactsForWielder();
		UpdateAbilities(base.Wielder, this);
	}

	public void UpdateAbilities(MechanicEntity wielderUnit, ItemEntity sourceItem)
	{
		base.Abilities.ForEach(delegate(Ability v)
		{
			wielderUnit.Facts.Remove(v);
		});
		PrepareAbilities(wielderUnit, sourceItem);
		base.Abilities.ForEach(delegate(Ability v)
		{
			wielderUnit.Facts.Add(v);
		});
	}

	public void PrepareAbilities(MechanicEntity wielderUnit, ItemEntity sourceItem)
	{
		base.Abilities.Clear();
		if (base.Blueprint == null || wielderUnit == null)
		{
			return;
		}
		base.Abilities.AddRange(base.Blueprint.WeaponAbilities.AllWithIndex.Where(((int Index, WeaponAbility Slot) i) => i.Slot.Ability != null).Select(delegate((int Index, WeaponAbility Slot) i)
		{
			Ability ability = new Ability(i.Slot.Ability, wielderUnit);
			if (ability != null)
			{
				ability.Data.ItemSlotIndex = i.Index;
			}
			return ability;
		}).NotNull());
		base.Abilities.ForEach(delegate(Ability v)
		{
			v.AddSource(sourceItem);
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<CountableFlag>.GetHash128(IsBlocked);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<ItemEntityStarshipAmmo>.GetHash128(FakeAmmo);
		result.Append(ref val3);
		return result;
	}
}
