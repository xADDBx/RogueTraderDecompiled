using System;
using System.Collections.Generic;
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
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
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

	public override PartUnitAlignment Alignment => GetRequired<PartUnitAlignment>();

	public override PartAbilityResourceCollection AbilityResources => GetRequired<PartAbilityResourceCollection>();

	public override PartUnitProgression Progression => GetRequired<PartUnitProgression>();

	public override PartUnitState State => GetRequired<PartUnitState>();

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
		GetOrCreate<PartProvidesFullCover>();
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

	public override ItemEntityWeapon GetPrimaryHandWeapon()
	{
		return Body.PrimaryHand.MaybeWeapon;
	}

	public override ItemEntityWeapon GetSecondaryHandWeapon()
	{
		return Body.SecondaryHand.MaybeWeapon;
	}

	public override ItemEntityShield GetShieldInHand()
	{
		return Body.SecondaryHand.MaybeShield;
	}

	protected override void OnNodeChanged()
	{
		base.OnNodeChanged();
		EventBus.RaiseEvent((IBaseUnitEntity)this, (Action<IUnitNodeChangedHandler>)delegate(IUnitNodeChangedHandler h)
		{
			h.HandleUnitNodeChanged();
		}, isCheckRuntime: true);
	}

	protected override void OnApplyPostLoadFixes()
	{
		try
		{
			List<Feature> list = Facts.GetAll<Feature>().ToTempList();
			List<Ability> list2 = Facts.GetAll<Ability>().ToTempList();
			if (list.Count > 0 || list2.Count > 0)
			{
				HashSet<string> hashSet = new HashSet<string>();
				foreach (ItemSlot allSlot in Body.AllSlots)
				{
					if (allSlot.MaybeItem != null && allSlot.Active)
					{
						hashSet.Add(allSlot.MaybeItem.UniqueId);
					}
				}
				while (list.Count > 0 || list2.Count > 0)
				{
					try
					{
						if (list.Count > 0)
						{
							Feature feature = list[0];
							IItemEntity sourceItem = feature.SourceItem;
							if (sourceItem != null && !hashSet.Contains(sourceItem.UniqueId))
							{
								Facts.Remove(feature);
							}
							list.RemoveAt(0);
						}
					}
					catch (Exception ex)
					{
						PFLog.Entity.Exception(ex);
					}
					try
					{
						if (list2.Count <= 0)
						{
							continue;
						}
						Ability ability = list2[list2.Count - 1];
						IItemEntity sourceItem2 = ability.SourceItem;
						EntityFactSource firstSource = ability.FirstSource;
						bool flag = sourceItem2 == null && firstSource == null;
						if ((sourceItem2 != null && !hashSet.Contains(sourceItem2.UniqueId)) || flag)
						{
							Facts.Remove(ability);
						}
						else
						{
							for (int num = list2.Count - 2; num >= 0; num--)
							{
								Ability ability2 = list2[num];
								IItemEntity sourceItem3 = ability2.SourceItem;
								EntityFactSource firstSource2 = ability2.FirstSource;
								bool flag2 = sourceItem3 == null && firstSource2 == null;
								bool flag3 = sourceItem3 != null && !hashSet.Contains(sourceItem3.UniqueId);
								if ((ability.Blueprint == ability2.Blueprint && sourceItem2 != null && sourceItem3 != null && sourceItem2.UniqueId.Equals(sourceItem3.UniqueId)) || flag3 || flag2)
								{
									Facts.Remove(ability2);
									list2.Remove(ability2);
								}
							}
						}
						list2.Remove(ability);
					}
					catch (Exception ex2)
					{
						PFLog.Entity.Exception(ex2);
					}
				}
			}
		}
		catch (Exception ex3)
		{
			PFLog.Entity.Exception(ex3);
		}
		base.OnApplyPostLoadFixes();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
