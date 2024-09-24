using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Sound;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Visual.FX;

public static class FXPlayer
{
	public static void Play([NotNull] IFXSettingsProvider effectsProvider, [NotNull] MechanicEntity caster, MappedAnimationEventType eventType, [CanBeNull] AbilityData ability = null)
	{
		IEnumerable<IFXSettings> fXs = effectsProvider.GetFXs(eventType);
		if (fXs == null)
		{
			return;
		}
		foreach (IFXSettings item in fXs)
		{
			Play(item, caster, null, ability);
		}
	}

	public static GameObject[] Play([NotNull] IFXSettingsProvider effectsProvider, [NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, AbilityEventType eventType, [CanBeNull] AbilityData ability = null)
	{
		IEnumerable<IFXSettings> fXs = effectsProvider.GetFXs(eventType);
		if (fXs == null || !fXs.Any())
		{
			return Array.Empty<GameObject>();
		}
		List<GameObject> list = new List<GameObject>();
		foreach (IFXSettings item in fXs)
		{
			list.AddRange(Play(item, caster, target, ability));
		}
		return list.ToArray();
	}

	public static GameObject[] Play(IFXSettings effect, MechanicEntity caster, [CanBeNull] TargetWrapper target, [CanBeNull] AbilityData ability = null)
	{
		if (effect.Settings.UseRandomVisualFX)
		{
			VisualFXSettings visualFXSettings = effect.Settings.FXs.Random(PFStatefulRandom.Visuals.Fx);
			if (visualFXSettings != null)
			{
				return PlayFX(visualFXSettings, caster, target, effect.Target, ability, effect.OverrideTargetOrientationSource);
			}
			return Array.Empty<GameObject>();
		}
		List<GameObject> list = new List<GameObject>();
		VisualFXSettings[] fXs = effect.Settings.FXs;
		foreach (VisualFXSettings fx in fXs)
		{
			list.AddRange(PlayFX(fx, caster, target, effect.Target, ability, effect.OverrideTargetOrientationSource));
		}
		return list.ToArray();
	}

	private static GameObject[] PlayFX(VisualFXSettings fx, MechanicEntity caster, [CanBeNull] TargetWrapper target, FXTarget targetType, [CanBeNull] AbilityData ability, bool overrideOrientationSource = false)
	{
		GameObject gameObject = fx.Prefab?.Load();
		SoundFx component;
		bool flag = ability?.FXSettings?.SoundFXSettings != null && gameObject != null && gameObject.TryGetComponent<SoundFx>(out component);
		switch (targetType)
		{
		case FXTarget.Caster:
		{
			GameObject[] array = new GameObject[1] { FxHelper.SpawnFxOnEntity(gameObject, caster.View, enableFxObject: true, overrideOrientationSource) };
			if (flag && array.Length != 0)
			{
				GameObject[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i].TryGetComponent<SoundFx>(out var component3))
					{
						component3.BlockSoundFXPlaying = true;
					}
				}
			}
			return array;
		}
		case FXTarget.Target:
		{
			GameObject[] array = ((target != null && target.Entity != null) ? new GameObject[1] { FxHelper.SpawnFxOnEntity(gameObject, target.Entity.View, enableFxObject: true, overrideOrientationSource) } : ((!(target != null) || !target.IsPoint) ? Array.Empty<GameObject>() : new GameObject[1] { FxHelper.SpawnFxOnPoint(gameObject, target.Point) }));
			if (flag && array.Length != 0)
			{
				GameObject[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i].TryGetComponent<SoundFx>(out var component2))
					{
						component2.BlockSoundFXPlaying = true;
					}
				}
			}
			return array;
		}
		case FXTarget.CasterWeapon:
		{
			WeaponSlot weaponSlot = (ability?.Weapon?.HoldingSlot ?? caster.GetFirstWeapon()?.HoldingSlot) as WeaponSlot;
			return new GameObject[1] { FxHelper.SpawnFxOnWeapon(gameObject, caster.View, weaponSlot?.FxSnapMap) };
		}
		case FXTarget.CasterOffHandWeapon:
		{
			WeaponSlot weaponSlot2 = caster.GetSecondaryHandWeapon()?.HoldingSlot as WeaponSlot;
			return new GameObject[1] { FxHelper.SpawnFxOnWeapon(gameObject, caster.View, weaponSlot2?.FxSnapMap) };
		}
		case FXTarget.CasterAllWeapon:
			if (caster is UnitEntity unitEntity)
			{
				List<GameObject> list = new List<GameObject>();
				foreach (HandsEquipmentSet handsEquipmentSet in unitEntity.Body.HandsEquipmentSets)
				{
					foreach (HandSlot hand in handsEquipmentSet.Hands)
					{
						if (hand?.FxSnapMap != null)
						{
							list.Add(FxHelper.SpawnFxOnWeapon(gameObject, caster.View, hand.FxSnapMap));
						}
					}
				}
				return list.ToArray();
			}
			return Array.Empty<GameObject>();
		default:
			throw new ArgumentOutOfRangeException("targetType", targetType, null);
		}
	}
}
