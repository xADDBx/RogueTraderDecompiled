using JetBrains.Annotations;
using Kingmaker.Localization;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.UI.Sound;

public static class VoiceOverPlayer
{
	[CanBeNull]
	public static VoiceOverStatus PlayVoiceOver([NotNull] LocalizedString text, [CanBeNull] MonoBehaviour target = null)
	{
		return PlayVoiceOver(text.GetVoiceOverSound(), target);
	}

	[CanBeNull]
	public static VoiceOverStatus PlayVoiceOver(string voiceOverSound, [CanBeNull] MonoBehaviour target = null)
	{
		if (string.IsNullOrEmpty(voiceOverSound))
		{
			return null;
		}
		GameObject gameObject = (target ? target.gameObject : SoundState.Get2DSoundObject());
		if (!gameObject)
		{
			return null;
		}
		SoundUtility.SetGenderFlags(gameObject);
		SoundUtility.SetRaceFlags(gameObject);
		VoiceOverStatus voiceOverStatus = new VoiceOverStatus(Game.Instance.Player.RealTime);
		uint num = SoundEventsManager.PostEvent(voiceOverSound, gameObject, 8u, voiceOverStatus.HandleCallback, null);
		if (num == 0)
		{
			return null;
		}
		voiceOverStatus.PlayingSoundId = num;
		return voiceOverStatus;
	}

	[CanBeNull]
	public static VoiceOverStatus PlayVoiceOver([NotNull] LocalizedString text, GameObject target)
	{
		return PlayVoiceOver(text.GetVoiceOverSound(), target);
	}

	[CanBeNull]
	public static VoiceOverStatus PlayVoiceOver(string voiceOverSound, GameObject target)
	{
		if (string.IsNullOrEmpty(voiceOverSound))
		{
			return null;
		}
		GameObject gameObject = ((target == null) ? SoundState.Get2DSoundObject() : target);
		if (!gameObject)
		{
			return null;
		}
		SoundUtility.SetGenderFlags(gameObject);
		SoundUtility.SetRaceFlags(gameObject);
		VoiceOverStatus voiceOverStatus = new VoiceOverStatus(Game.Instance.Player.RealTime);
		uint num = SoundEventsManager.PostEvent(voiceOverSound, gameObject, 8u, voiceOverStatus.HandleCallback, null);
		if (num == 0)
		{
			return null;
		}
		voiceOverStatus.PlayingSoundId = num;
		return voiceOverStatus;
	}
}
