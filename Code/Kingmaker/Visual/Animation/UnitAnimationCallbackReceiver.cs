using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums.Sound;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[DisallowMultipleComponent]
public class UnitAnimationCallbackReceiver : MonoBehaviour
{
	[CanBeNull]
	private AbstractUnitEntityView m_UnitView;

	[CanBeNull]
	private UnitAnimationManager m_AnimationManager;

	[UsedImplicitly]
	private void OnEnable()
	{
		m_UnitView = GetComponentInParent<AbstractUnitEntityView>();
		m_AnimationManager = GetComponentInParent<UnitAnimationManager>();
	}

	public uint PostEvent(string eventName)
	{
		return PostEvent(eventName, 1f, withPrefix: false);
	}

	public uint PostEvent(string eventName, float volume)
	{
		return PostEvent(eventName, volume, withPrefix: false);
	}

	public uint PostEventWithPrefix(string eventName)
	{
		return PostEventWithPrefix(eventName, 1f);
	}

	public uint PostEventWithPrefix(string eventName, float volume)
	{
		return PostEvent(eventName, volume, withPrefix: true);
	}

	public uint PostEvent(string eventName, float volume, bool withPrefix)
	{
		if (m_UnitView == null)
		{
			m_UnitView = GetComponentInParent<UnitEntityView>();
		}
		if (m_UnitView != null)
		{
			if (m_UnitView.EntityData == null)
			{
				PFLog.Default.Error(this, $"No unit for view {m_UnitView}");
				return 0u;
			}
			float in_value = ((!m_UnitView.EntityData.IsInFogOfWar) ? 1f : (m_UnitView.EntityData.IsRevealed ? 0.5f : 0f));
			AkSoundEngine.SetRTPCValue("Audibility", in_value, m_UnitView.gameObject);
			float in_value2 = m_UnitView.AnimationManager?.CurrentAction?.SpeedScale ?? 1f;
			AkSoundEngine.SetRTPCValue("CombatSpeed", in_value2, m_UnitView.gameObject);
			return SoundEventsManager.PostEvent((withPrefix ? m_UnitView.Blueprint.VisualSettings.FoleySoundPrefix : "") + eventName, m_UnitView.gameObject);
		}
		return 0u;
	}

	public void StopPlayingById(uint playingId)
	{
		SoundEventsManager.StopPlayingById(playingId);
	}

	public void PostEventMapped(MappedAnimationEventType evt)
	{
		if (!(m_UnitView == null))
		{
			AbstractUnitEntity data = m_UnitView.Data;
			if (data != null && !data.IsDisposed)
			{
				(m_UnitView.Asks?.SelectAnimationBark(evt))?.Schedule();
			}
		}
	}

	public void AbilityAnimationEvent(UnitSoundAnimationEventType eventType)
	{
		if (m_UnitView == null || !(m_UnitView is UnitEntityView unitEntityView))
		{
			return;
		}
		BlueprintAbility blueprintAbility = ((UnitAnimationActionHandle)(unitEntityView.AnimationManager?.CurrentAction))?.Spell;
		if (blueprintAbility != null)
		{
			BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = (unitEntityView.Data.Abilities.GetAbility(blueprintAbility)?.Data.FXSettings ?? blueprintAbility.FXSettings)?.SoundFXSettings;
			if (blueprintAbilitySoundFXSettings != null && unitEntityView.Data.Context.MaybeCaster != null)
			{
				SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, unitEntityView.Data.Context.MaybeCaster, eventType);
			}
		}
	}

	public void PostMainWeaponEquipEvent()
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView)
		{
			ItemEntityWeapon itemEntityWeapon = unitEntityView.EntityData?.Body.PrimaryHand.MaybeWeapon;
			if (itemEntityWeapon != null)
			{
				string equipSound = itemEntityWeapon.Blueprint.VisualParameters.EquipSound;
				PostEvent(equipSound, 1f, withPrefix: false);
			}
		}
	}

	public void PostOffWeaponEquipEvent()
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView)
		{
			ItemEntityWeapon maybeWeapon = unitEntityView.EntityData.Body.SecondaryHand.MaybeWeapon;
			if (maybeWeapon != null)
			{
				string equipSound = maybeWeapon.Blueprint.VisualParameters.EquipSound;
				PostEvent(equipSound, 1f, withPrefix: false);
			}
		}
	}

	public void PostMainWeaponUnequipEvent()
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView)
		{
			ItemEntity itemEntity = (unitEntityView.HandsEquipment.IsMainHandMismatched ? unitEntityView.EntityData.Body.HandsEquipmentSets[(unitEntityView.EntityData.Body.CurrentHandEquipmentSetIndex + 1) % 2].PrimaryHand.MaybeItem : unitEntityView.EntityData?.Body.PrimaryHand.MaybeItem);
			if (itemEntity != null && itemEntity.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
			{
				string unequipSound = blueprintItemWeapon.VisualParameters.UnequipSound;
				PostEvent(unequipSound, 1f, withPrefix: false);
			}
		}
	}

	public void PostOffWeaponUnequipEvent()
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView)
		{
			ItemEntity itemEntity = (unitEntityView.HandsEquipment.IsOffHandMismatched ? unitEntityView.EntityData.Body.HandsEquipmentSets[(unitEntityView.EntityData.Body.CurrentHandEquipmentSetIndex + 1) % 2].SecondaryHand.MaybeItem : unitEntityView.EntityData?.Body.SecondaryHand.MaybeItem);
			if (itemEntity != null && itemEntity.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
			{
				string unequipSound = blueprintItemWeapon.VisualParameters.UnequipSound;
				PostEvent(unequipSound, 1f, withPrefix: false);
			}
		}
	}

	public void PostArmorFoleyEvent()
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView && unitEntityView.EntityData?.Body.Armor.MaybeItem is ItemEntityArmor itemEntityArmor)
		{
			string animationFoleySound = itemEntityArmor.Blueprint.VisualParameters.AnimationFoleySound;
			PostEvent(animationFoleySound, 1f, withPrefix: false);
		}
	}

	public void PostEventWithSurface(string eventName)
	{
		if (!(m_UnitView == null))
		{
			SpawnDustFx(eventName);
			SetTerrainSwitch();
			PostEvent(eventName, 1f, withPrefix: false);
		}
	}

	public void PlayBodyFall(string eventName)
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView)
		{
			SpawnDustFx(eventName);
			AkSwitchReference akSwitchReference = unitEntityView.EntityData?.Body.Armor.MaybeArmor?.Blueprint.VisualParameters.SoundSwitch;
			if (akSwitchReference != null && !akSwitchReference.Value.IsNullOrEmpty())
			{
				akSwitchReference.Set(unitEntityView.gameObject);
			}
			else
			{
				unitEntityView.Blueprint.VisualSettings.BodyTypeSoundSwitch.Set(unitEntityView.gameObject);
			}
			unitEntityView.Blueprint.VisualSettings.BodySizeSoundSwitch.Set(unitEntityView.gameObject);
			SetTerrainSwitch();
			PostEvent(eventName, 1f, withPrefix: false);
		}
	}

	public void PlayFootstep(string eventName)
	{
		AbstractUnitEntityView unitView = m_UnitView;
		if (!(unitView == null))
		{
			SpawnDustFx(eventName);
			unitView.Blueprint.VisualSettings.FootTypeSoundSwitch.Set(unitView.gameObject);
			unitView.Blueprint.VisualSettings.FootSizeSoundSwitch.Set(unitView.gameObject);
			SetTerrainSwitch();
			PostEvent(eventName, 1f, withPrefix: false);
		}
	}

	private void SpawnDustFx(string eventName)
	{
		if (BlueprintRoot.Instance.FxRoot.FallEventStrings.Any((string s) => s == eventName))
		{
			FxHelper.SpawnFxOnEntity(BlueprintRoot.Instance.FxRoot.DustOnFallPrefab, m_UnitView);
		}
	}

	private void SetTerrainSwitch()
	{
		SurfaceType? surfaceSoundTypeSwitch = SurfaceTypeObject.GetSurfaceSoundTypeSwitch(base.transform.position);
		if (surfaceSoundTypeSwitch.HasValue)
		{
			string text = surfaceSoundTypeSwitch.ToString();
			if (!text.IsNullOrEmpty())
			{
				AkSoundEngine.SetSwitch("Terrain", text, m_UnitView.gameObject);
			}
		}
	}

	public void AnimateWeaponTrail(float duration)
	{
		if (m_UnitView != null && m_UnitView is UnitEntityView unitEntityView)
		{
			unitEntityView.AnimateWeaponTrail(duration);
		}
	}

	public void FxAnimatorToggleAction(string objectName)
	{
		Transform transform = base.transform.parent.FindChildRecursive(objectName);
		if (transform != null && transform.GetComponent<Animator>() != null)
		{
			transform.GetComponent<Animator>().enabled = true;
		}
	}

	public void PostCommandActEvent()
	{
		m_AnimationManager?.OnCommandActEvent();
	}

	public void PostDecoratorObject(UnitAnimationDecoratorObject decorator)
	{
		if (m_UnitView == null)
		{
			m_UnitView = GetComponentInParent<AbstractUnitEntityView>();
		}
		if (decorator != null && decorator.Duration > 0f && decorator.Entries.Length != 0 && m_AnimationManager != null && m_UnitView != null)
		{
			UnitAnimationDecoratorManager decoratorManager = m_AnimationManager.DecoratorManager;
			if (!decorator.UseGender || m_UnitView.Blueprint.Gender == decorator.gender)
			{
				decoratorManager.ShowDecorator(decorator, m_UnitView);
			}
		}
	}

	public void PlaceFootprintEvent(string locator, int footIndex)
	{
		AbstractUnitEntityView unitView = m_UnitView;
		if (unitView == null)
		{
			return;
		}
		AbstractUnitEntity entityData = unitView.EntityData;
		if (entityData != null)
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)entityData, (Action<IUnitFootstepAnimationEventHandler>)delegate(IUnitFootstepAnimationEventHandler h)
			{
				h.HandleUnitFootstepAnimationEvent(locator, footIndex);
			}, isCheckRuntime: true);
		}
	}

	public void UnhideTorchEvent()
	{
		_ = m_UnitView == null;
	}

	public void ChangeShowingWeapon(bool isVisible)
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment?.GetSelectedWeaponSet()?.MainHand?.ShowItem(isVisible);
		}
	}

	public void ChangeAttachPointForMainHandWeapon(bool inMainHand)
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView && !(unitEntityView.HandsEquipment?.GetSelectedWeaponSet()?.MainHand?.VisualModel == null))
		{
			unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.VisualModel.transform.SetParent(inMainHand ? unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.MainHandTransform : unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.OffHandTransform, worldPositionStays: true);
		}
	}
}
