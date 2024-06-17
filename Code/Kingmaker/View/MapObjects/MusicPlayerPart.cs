using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class MusicPlayerPart : ViewBasedPart<MusicPlayerSettings>, IAreaHandler, ISubscriber, IHashable
{
	private bool m_IsLoaded;

	[JsonProperty]
	public bool IsPlaying { get; private set; }

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		m_IsLoaded = true;
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		if (!m_IsLoaded)
		{
			IsPlaying = base.Settings.AutoPlay;
		}
	}

	public void OnAreaBeginUnloading()
	{
		if (IsPlaying)
		{
			SetPlayingInternal(isPlaying: false);
		}
	}

	public void OnAreaDidLoad()
	{
		if (IsPlaying)
		{
			SetPlayingInternal(isPlaying: true);
		}
	}

	public void SetPlaying(bool isPlaying)
	{
		if (isPlaying != IsPlaying)
		{
			IsPlaying = isPlaying;
			SetPlayingInternal(isPlaying);
		}
	}

	private void SetPlayingInternal(bool isPlaying)
	{
		if (isPlaying)
		{
			string.IsNullOrEmpty(base.Settings.Start);
			string[] startEvents = base.Settings.StartEvents;
			for (int i = 0; i < startEvents.Length; i++)
			{
				SoundEventsManager.PostEvent(startEvents[i], ((EntityViewBase)base.View).Or(null)?.gameObject);
			}
		}
		else
		{
			string.IsNullOrEmpty(base.Settings.Start);
			string[] startEvents = base.Settings.StopEvents;
			for (int i = 0; i < startEvents.Length; i++)
			{
				SoundEventsManager.PostEvent(startEvents[i], ((EntityViewBase)base.View).Or(null)?.gameObject);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsPlaying;
		result.Append(ref val2);
		return result;
	}
}
