using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.Visual.Particles;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("c788cf748ec86bc45b986ac4de28cf45")]
public class AbilitySpawnFx : BlueprintComponent, IResourcesHolder
{
	public PrefabLink PrefabLink;

	public AbilitySpawnFxTime Time;

	public AbilitySpawnFxAnchor Anchor;

	public AbilitySpawnFxWeaponTarget WeaponTarget;

	public bool DestroyOnCast;

	public float Delay;

	[Header("Position")]
	public AbilitySpawnFxAnchor PositionAnchor = AbilitySpawnFxAnchor.None;

	public bool UseExtraAttachment;

	[ShowIf("UseExtraAttachment")]
	public int AttachingSnapperIndex;

	[Header("Orientation")]
	public AbilitySpawnFxAnchor OrientationAnchor = AbilitySpawnFxAnchor.None;

	public AbilitySpawnFxOrientation OrientationMode;

	public bool UsesSelectedTarget
	{
		get
		{
			if (Anchor != AbilitySpawnFxAnchor.SelectedTarget && PositionAnchor != AbilitySpawnFxAnchor.SelectedTarget)
			{
				return OrientationAnchor == AbilitySpawnFxAnchor.SelectedTarget;
			}
			return true;
		}
	}

	public void Spawn([NotNull] MechanicsContext context, [CanBeNull] TargetWrapper selectedTarget, List<GameObject> list = null)
	{
		if (context.DisableFx || ((selectedTarget != null) ^ UsesSelectedTarget))
		{
			return;
		}
		TargetWrapper targetWrapper = ResolveAnchor(Anchor, context, selectedTarget);
		if (!(targetWrapper == null))
		{
			if (Delay <= 0f)
			{
				DoSpawn(context, selectedTarget, list, targetWrapper);
			}
			else if (context.MaybeCaster?.View != null)
			{
				Game.Instance.CoroutinesController.Start(DoSpawnDelayed(context, selectedTarget, list, targetWrapper), context.MaybeCaster.View);
			}
		}
		IEnumerator DoSpawnDelayed(MechanicsContext context, TargetWrapper selectedTarget, List<GameObject> list, TargetWrapper target)
		{
			yield return YieldInstructions.WaitForSecondsGameTime(Delay);
			DoSpawn(context, selectedTarget, list, target);
		}
	}

	private void DoSpawn(MechanicsContext context, TargetWrapper selectedTarget, List<GameObject> list, TargetWrapper target)
	{
		GameObject prefab = PrefabLink.Load();
		if (prefab == null)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IAbilitySoundZoneTrigger h)
		{
			h.TriggerSoundZone(context, prefab);
		});
		GameObject gameObject = null;
		if (target.Entity != null)
		{
			if (target.Entity is StarshipEntity && context is AbilityExecutionContext abilityExecutionContext)
			{
				ItemEntityStarshipWeapon weapon = abilityExecutionContext.Ability.StarshipWeapon;
				List<StarshipFxLocator> list2 = abilityExecutionContext.Caster.View.gameObject.GetComponentsInChildren<StarshipFxLocator>().ToList();
				Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot weaponSlot = weapon.HoldingSlot as Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot;
				foreach (StarshipFxLocator item in list2.FindAll((StarshipFxLocator x) => x.weaponSlotType == weaponSlot.Type && x.starshipWeaponType == weapon.Blueprint.WeaponType))
				{
					gameObject = FxHelper.SpawnFxOnWeapon(prefab, target.Entity.View, item.particleMap as WeaponParticlesSnapMap);
					list?.Add(gameObject);
					PositionFx(context, selectedTarget, gameObject);
				}
			}
			else if (target.Entity is UnitEntity unitEntity)
			{
				switch (WeaponTarget)
				{
				case AbilitySpawnFxWeaponTarget.None:
					gameObject = FxHelper.SpawnFxOnEntity(prefab, target.Entity.View);
					break;
				case AbilitySpawnFxWeaponTarget.Primary:
					gameObject = FxHelper.SpawnFxOnWeapon(prefab, target.Entity.View, unitEntity.Body.PrimaryHand.FxSnapMap);
					break;
				case AbilitySpawnFxWeaponTarget.Secondary:
					gameObject = FxHelper.SpawnFxOnWeapon(prefab, target.Entity.View, unitEntity.Body.SecondaryHand.FxSnapMap);
					break;
				case AbilitySpawnFxWeaponTarget.All:
				{
					foreach (Kingmaker.Items.Slots.WeaponSlot item2 in unitEntity.Body.AllSlots.OfType<Kingmaker.Items.Slots.WeaponSlot>())
					{
						if (item2.HasWeapon)
						{
							gameObject = FxHelper.SpawnFxOnWeapon(prefab, target.Entity.View, item2.FxSnapMap);
							if ((bool)gameObject)
							{
								list?.Add(gameObject);
								PositionFx(context, selectedTarget, gameObject);
							}
						}
					}
					return;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		else
		{
			Quaternion rotation = Quaternion.Euler(0f, target.Orientation, 0f);
			gameObject = FxHelper.SpawnFxOnPoint(prefab, target.Point, rotation);
		}
		if ((bool)gameObject)
		{
			PositionFx(context, selectedTarget, gameObject);
			list?.Add(gameObject);
		}
	}

	private void PositionFx(MechanicsContext context, TargetWrapper selectedTarget, GameObject fx)
	{
		if (!fx)
		{
			return;
		}
		TargetWrapper targetWrapper = ResolveAnchor(PositionAnchor, context, selectedTarget);
		if (targetWrapper != null)
		{
			fx.transform.position = targetWrapper.Point;
			if (UseExtraAttachment)
			{
				FxHelper.Resnap(fx, targetWrapper.Entity?.View.gameObject, AttachingSnapperIndex);
			}
		}
		TargetWrapper targetWrapper2 = ResolveAnchor(OrientationAnchor, context, selectedTarget);
		if (targetWrapper2 != null)
		{
			switch (OrientationMode)
			{
			case AbilitySpawnFxOrientation.Copy:
				fx.transform.rotation = Quaternion.Euler(0f, targetWrapper2.Orientation, 0f);
				break;
			case AbilitySpawnFxOrientation.TurnFrom:
				fx.transform.rotation = Quaternion.LookRotation(fx.transform.position - targetWrapper2.Point);
				break;
			case AbilitySpawnFxOrientation.TurnTo:
				fx.transform.rotation = Quaternion.LookRotation(targetWrapper2.Point - fx.transform.position);
				break;
			default:
				PFLog.Default.Error("Unknown orientation mode {0}", OrientationMode);
				break;
			}
		}
	}

	[CanBeNull]
	private static TargetWrapper ResolveAnchor(AbilitySpawnFxAnchor anchor, [NotNull] MechanicsContext context, [CanBeNull] TargetWrapper selectedTarget)
	{
		switch (anchor)
		{
		case AbilitySpawnFxAnchor.None:
			return null;
		case AbilitySpawnFxAnchor.Caster:
			if (context.MaybeCaster == null)
			{
				PFLog.Default.Error("Caster is missing");
				return null;
			}
			return context.MaybeCaster;
		case AbilitySpawnFxAnchor.ClickedTarget:
			if (!(context is AbilityExecutionContext abilityExecutionContext))
			{
				return selectedTarget;
			}
			return abilityExecutionContext.ClickedTarget;
		case AbilitySpawnFxAnchor.SelectedTarget:
			return selectedTarget;
		default:
			PFLog.Default.Error("Unknown anchor {0}", anchor);
			return null;
		}
	}
}
