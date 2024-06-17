using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.FX;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public static class SoundEventPlayer
{
	public static void Play([NotNull] ISoundSettingsProvider soundProvider, [NotNull] MechanicEntity caster, UnitSoundAnimationEventType eventType)
	{
		IEnumerable<ISoundSettings> sounds = soundProvider.GetSounds(eventType);
		if (sounds == null)
		{
			return;
		}
		foreach (ISoundSettings item in sounds)
		{
			ISoundSettings effect = item;
			if (effect.Settings.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow(effect, caster);
			}
			IEnumerator PlaySoundDelayed()
			{
				yield return new WaitForSeconds(effect.Settings.Delay);
				PlaySoundNow(effect, caster);
			}
		}
	}

	public static void Play([NotNull] ISoundSettingsProvider soundProvider, [NotNull] MechanicsContext context, AbilityEventType eventType)
	{
		IEnumerable<ISoundSettings> sounds = soundProvider.GetSounds(eventType);
		if (sounds == null || context.MaybeCaster == null)
		{
			return;
		}
		foreach (ISoundSettings item in sounds)
		{
			ISoundSettings effect = item;
			GameObject targetObject = GetSoundTarget(context.MaybeCaster, context.MainTarget, effect.Target);
			if (effect.Settings.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow(effect, context.MaybeCaster, targetObject);
			}
			IEnumerator PlaySoundDelayed()
			{
				yield return new WaitForSeconds(effect.Settings.Delay);
				PlaySoundNow(effect, context.MaybeCaster, targetObject);
			}
		}
	}

	public static void Play([NotNull] ISoundSettingsProvider soundProvider, [NotNull] MechanicEntity entity, [NotNull] TargetWrapper target, AbilityEventType eventType)
	{
		IEnumerable<ISoundSettings> sounds = soundProvider.GetSounds(eventType);
		if (sounds == null)
		{
			return;
		}
		foreach (ISoundSettings item in sounds)
		{
			ISoundSettings effect = item;
			GameObject targetObject = GetSoundTarget(entity, target, effect.Target);
			if (effect.Settings.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow(effect, entity, targetObject, target?.Entity);
			}
			IEnumerator PlaySoundDelayed()
			{
				yield return new WaitForSeconds(effect.Settings.Delay);
				PlaySoundNow(effect, entity, targetObject);
			}
		}
	}

	private static void PlaySoundNow(ISoundSettings effect, MechanicEntity entity)
	{
		uint id = SoundEventsManager.PostEvent(effect.Settings.Event, entity.View.gameObject);
		PlaySoundWithId(effect, entity, id);
	}

	private static void PlaySoundNow(ISoundSettings effect, MechanicEntity entity, GameObject gameObject, [CanBeNull] MechanicEntity target = null)
	{
		if (!(gameObject == null))
		{
			uint id = SoundEventsManager.PostEvent(effect.Settings.Event, gameObject);
			PlaySoundWithId(effect, entity, id, target);
		}
	}

	private static void PlaySoundWithId(ISoundSettings effect, MechanicEntity entity, uint id, [CanBeNull] MechanicEntity target = null)
	{
		if (id != 0)
		{
			AkSoundEngine.SetRTPCValueByPlayingID("SpellGain", effect.Settings.Gain, id);
			AkSoundEngine.SetRTPCValueByPlayingID("SpellPitch", effect.Settings.Pitch, id);
			GameSyncSettings[] gameSyncs = effect.Settings.GameSyncs;
			for (int i = 0; i < gameSyncs.Length; i++)
			{
				gameSyncs[i]?.Sync(new PropertyContext(entity, null, target), id);
			}
		}
	}

	public static void PlaySound(SoundFXSettings sound, GameObject gameObject)
	{
		if (AkSoundEngine.IsInitialized())
		{
			if (sound.Delay > 0f)
			{
				MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PlaySoundDelayed());
			}
			else
			{
				PlaySoundNow();
			}
		}
		IEnumerator PlaySoundDelayed()
		{
			yield return new WaitForSeconds(sound.Delay);
			PlaySoundNow();
		}
		void PlaySoundNow()
		{
			uint num = SoundEventsManager.PostEvent(sound.Event, gameObject);
			if (num != 0)
			{
				AkSoundEngine.SetRTPCValueByPlayingID("SpellGain", sound.Gain, num);
				AkSoundEngine.SetRTPCValueByPlayingID("SpellPitch", sound.Pitch, num);
			}
		}
	}

	private static GameObject GetSoundTarget(MechanicEntity caster, [CanBeNull] TargetWrapper target, FXTarget targetType)
	{
		switch (targetType)
		{
		case FXTarget.Caster:
			if (caster is SimpleCaster simpleCaster2 && simpleCaster2.TrapParentObject != null)
			{
				return simpleCaster2.TrapParentObject;
			}
			return caster.View.Or(null)?.gameObject;
		case FXTarget.Target:
		{
			if ((object)target != null && target.IsPoint)
			{
				GameObject gameObject = new GameObject("SoundTarget_" + target.Point.ToString());
				gameObject.transform.SetPositionAndRotation(target.Point, Quaternion.identity);
				gameObject.AddComponent<AutoDestroy>().Lifetime = 5f;
				return gameObject;
			}
			object obj = target?.Entity?.View.Or(null)?.gameObject.Or(null);
			if (obj == null)
			{
				MechanicEntityView mechanicEntityView = caster.View.Or(null);
				if ((object)mechanicEntityView == null)
				{
					return null;
				}
				obj = mechanicEntityView.gameObject;
			}
			return (GameObject)obj;
		}
		case FXTarget.CasterWeapon:
			if (caster is SimpleCaster simpleCaster && simpleCaster.TrapParentObject != null)
			{
				return simpleCaster.TrapParentObject;
			}
			return caster.View.Or(null)?.gameObject;
		case FXTarget.CasterAllWeapon:
			return null;
		case FXTarget.CasterOffHandWeapon:
			return null;
		default:
			throw new ArgumentOutOfRangeException("targetType", targetType, null);
		}
	}
}
