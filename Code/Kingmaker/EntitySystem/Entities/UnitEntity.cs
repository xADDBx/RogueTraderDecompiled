using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class UnitEntity : BaseUnitEntity, PartMomentum.IOwner, IEntityPartOwner<PartMomentum>, IEntityPartOwner, IAreaHandler, ISubscriber, IUnitEntity, IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable, IHashable
{
	public override Type RequiredBlueprintType => typeof(BlueprintUnitFact);

	public override bool BlockOccupiedNodes => base.LifeState.IsConscious;

	public override PartInventory Inventory => GetRequired<PartInventory>();

	public override PartFaction Faction => GetRequired<PartFaction>();

	public override PartUnitProficiency Proficiencies => GetRequired<PartUnitProficiency>();

	public override PartUnitBody Body => GetRequired<PartUnitBody>();

	public override PartAbilityResourceCollection AbilityResources => GetRequired<PartAbilityResourceCollection>();

	public override PartUnitProgression Progression => GetRequired<PartUnitProgression>();

	public override PartUnitState State => GetRequired<PartUnitState>();

	public override PartUnitAlignment Alignment => GetRequired<PartUnitAlignment>();

	public override PartUnitBrain Brain => GetRequired<PartUnitBrain>();

	public override PartUnitAsks Asks => GetRequired<PartUnitAsks>();

	public override PartUnitViewSettings ViewSettings => GetRequired<PartUnitViewSettings>();

	public override PartUnitDescription Description => GetRequired<PartUnitDescription>();

	public override PartVision Vision => GetRequired<PartVision>();

	public override PartUnitStealth Stealth => GetRequired<PartUnitStealth>();

	public override PartUnitCombatState CombatState => GetRequired<PartUnitCombatState>();

	public override PartCombatGroup CombatGroup => GetRequired<PartCombatGroup>();

	public override PartStatsAttributes Attributes => GetRequired<PartStatsAttributes>();

	public override PartStatsSkills Skills => GetRequired<PartStatsSkills>();

	public override PartStatsSaves Saves => GetRequired<PartStatsSaves>();

	public override PartHealth Health => GetRequired<PartHealth>();

	public PartMomentum Momentum => GetRequired<PartMomentum>();

	public UnitEntity(UnitEntityView view)
		: this(view.UniqueId, view.IsInGameBySettings, view.Blueprint)
	{
	}

	public UnitEntity(string uniqueId, bool isInGame, BlueprintUnit blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected UnitEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartUnitViewSettings>();
		GetOrCreate<PartUnitAlignment>();
		GetOrCreate<PartUnitCommands>();
		GetOrCreate<PartUnitCombatState>();
		GetOrCreate<PartFaction>();
		GetOrCreate<PartCombatGroup>();
		GetOrCreate<PartVision>();
		GetOrCreate<PartUnitStealth>();
		GetOrCreate<PartStatsAttributes>();
		GetOrCreate<PartStatsSkills>();
		GetOrCreate<PartStatsSaves>();
		GetOrCreate<PartMomentum>();
		GetOrCreate<PartUnitProgression>();
		GetOrCreate<PartUnitState>();
		GetOrCreate<PartHealth>();
		GetOrCreate<PartLifeState>();
		GetOrCreate<PartMovable>();
		GetOrCreate<PartUnitProficiency>();
		GetOrCreate<PartAbilityResourceCollection>();
		GetOrCreate<PartInventory>();
		GetOrCreate<PartUnitBody>();
		GetOrCreate<PartUnitBrain>();
		GetOrCreate<PartUnitAsks>();
		GetOrCreate<PartUnitDescription>();
		GetOrCreate<PartTwoWeaponFighting>();
		GetOrCreate<UnitPronePart>();
		if (Faction.IsPlayer)
		{
			GetOrCreate<UnitPartPartyWeatherBuff>();
		}
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAreaBeginUnloading()
	{
		if (base.LifeState.IsFinallyDead && !base.Features.SuppressedDecomposition && !IsDeadAndHasLoot && !Faction.IsPlayer)
		{
			Game.Instance.EntityDestroyer.Destroy(this);
		}
	}

	public override ItemEntityWeapon GetFirstWeapon()
	{
		ItemEntityWeapon maybeWeapon = Body.PrimaryHand.MaybeWeapon;
		if (maybeWeapon == null)
		{
			maybeWeapon = Body.SecondaryHand.MaybeWeapon;
			if (maybeWeapon == null)
			{
				WeaponSlot weaponSlot = Body.AdditionalLimbs.FirstItem((WeaponSlot l) => l.MaybeWeapon != null);
				if (weaponSlot == null)
				{
					return null;
				}
				maybeWeapon = weaponSlot.MaybeWeapon;
			}
		}
		return maybeWeapon;
	}

	public override ItemEntityWeapon GetSecondWeapon()
	{
		return Body.SecondaryHand.MaybeWeapon;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
