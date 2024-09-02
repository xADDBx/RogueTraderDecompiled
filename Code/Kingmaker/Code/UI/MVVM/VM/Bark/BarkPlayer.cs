using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Sound.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public static class BarkPlayer
{
	public static IBarkHandle Bark(Entity entity, LocalizedString text, string encyclopediaLink, float duration = -1f, bool playVoiceOver = false)
	{
		string voiceOver = (playVoiceOver ? text.GetVoiceOverSound() : null);
		return BarkExploration(entity, text.Text, encyclopediaLink, duration, voiceOver);
	}

	public static IBarkHandle Bark(Entity entity, LocalizedString text, float duration = -1f, bool playVoiceOver = false, BaseUnitEntity interactUser = null, bool synced = true, string overrideName = null, Color overrideNameColor = default(Color))
	{
		if (string.IsNullOrEmpty(text.Text) && !NetworkingManager.IsMultiplayer)
		{
			return null;
		}
		string voiceOver = (playVoiceOver ? text.GetVoiceOverSound() : null);
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			return BarkExploration(entity, text.Text, duration, voiceOver);
		}
		return Bark(entity, text.Text, duration, voiceOver, interactUser, synced, overrideName, overrideNameColor);
	}

	public static IBarkHandle Bark(Entity entity, string text, float duration = -1f, string voiceOver = null, BaseUnitEntity interactUser = null, bool synced = true, string overrideName = null, Color overrideNameColor = default(Color))
	{
		if (entity == null)
		{
			if (string.IsNullOrEmpty(overrideName))
			{
				return new BarkHandle(entity, text, duration);
			}
			return new BarkHandle(entity, text, overrideName, overrideNameColor, duration);
		}
		if (entity != null && !entity.IsInGame)
		{
			return null;
		}
		VoiceOverStatus voiceOverStatus = ((voiceOver != null) ? VoiceOverPlayer.PlayVoiceOver(voiceOver, entity.View?.GO) : null);
		if (entity.IsVisibleForPlayer)
		{
			EventBus.RaiseEvent((IEntity)(entity ?? Game.Instance.Player.MainCharacterEntity), (Action<ICombatLogBarkHandler>)delegate(ICombatLogBarkHandler h)
			{
				h.HandleOnShowBark(text);
			}, isCheckRuntime: true);
		}
		if (entity == null || !entity.IsVisibleForPlayer)
		{
			return null;
		}
		if (string.IsNullOrEmpty(overrideName))
		{
			return new BarkHandle(entity, text, duration, voiceOverStatus, synced);
		}
		return new BarkHandle(entity, text, overrideName, overrideNameColor, duration, voiceOverStatus, synced);
	}

	public static IBarkHandle BarkSubtitle(MechanicEntity entity, string text, float duration = -1f, LocalizedString speakerName = null)
	{
		return new SubtitleBarkHandle(GetSubtitleText(entity, speakerName, text), duration, VoiceOverPlayer.PlayVoiceOver(text));
	}

	public static IBarkHandle BarkSubtitle(MechanicEntity entity, LocalizedString text, float duration = -1f, bool durationByVoice = false, LocalizedString speakerName = null)
	{
		return new SubtitleBarkHandle(GetSubtitleText(entity, speakerName, text.Text), duration, VoiceOverPlayer.PlayVoiceOver(text));
	}

	private static string GetSubtitleText([CanBeNull] MechanicEntity entity, [CanBeNull] LocalizedString speakerName, string text)
	{
		if (speakerName != null && !speakerName.IsEmpty())
		{
			return speakerName.Text + ": " + text;
		}
		if (entity == null)
		{
			return text;
		}
		return entity.Name + ": " + text;
	}

	public static IBarkHandle BarkSubtitle(LocalizedString text, float duration = -1f, LocalizedString speakerName = null)
	{
		return BarkSubtitle(text.Text, duration, speakerName);
	}

	public static IBarkHandle BarkSubtitle(string text, float duration = -1f, LocalizedString speakerName = null)
	{
		return new SubtitleBarkHandle(GetSubtitleText(speakerName, text), duration, VoiceOverPlayer.PlayVoiceOver(text));
	}

	private static string GetSubtitleText(LocalizedString speakerName, string text)
	{
		if (speakerName != null && !speakerName.IsEmpty())
		{
			return speakerName.Text + ": " + text;
		}
		return text;
	}

	private static IBarkHandle BarkExploration(Entity entity, string text, float duration = -1f, string voiceOver = null)
	{
		VoiceOverStatus voiceOverStatus = ((voiceOver != null) ? VoiceOverPlayer.PlayVoiceOver(voiceOver, entity?.View?.GO) : null);
		return new BarkHandle(entity, text, duration, voiceOverStatus);
	}

	private static IBarkHandle BarkExploration(Entity entity, string text, string encyclopediaLink, float duration = -1f, string voiceOver = null)
	{
		VoiceOverStatus voiceOverStatus = ((voiceOver != null) ? VoiceOverPlayer.PlayVoiceOver(voiceOver, entity?.View?.GO) : null);
		return new BarkHandle(entity, text, encyclopediaLink, duration, voiceOverStatus);
	}
}
