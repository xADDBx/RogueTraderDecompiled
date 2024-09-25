using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Signals;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.FlagCountable;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("435726b96307405c8b721e942c7db8d4")]
public class CommandPlayVideo : CommandBase
{
	private class Data
	{
		internal bool IsSkipped;

		internal SignalWrapper StopPlaySignal;

		internal InterchapterData Interchapter;

		internal bool Finished => Interchapter.Finished;

		internal double TimeSinceStateStart => (Game.Instance.Player.RealTime - Interchapter.StateStartTime).TotalSeconds;
	}

	public static readonly CountableFlag Flag = new CountableFlag();

	public static readonly double MaxPreparingTime = TimeSpan.FromMinutes(2.0).TotalSeconds;

	public static readonly double MaxPlayingLagTime = TimeSpan.FromMinutes(2.0).TotalSeconds;

	[SerializeField]
	[ValidateNotNull]
	private VideoClip m_VideoClip;

	[SerializeField]
	[ValidateNotNull]
	private VideoClip m_VideoClip4K;

	[SerializeField]
	private string m_SoundStartEventName;

	[SerializeField]
	private bool m_StopSoundByEvent;

	[SerializeField]
	[ShowIf("m_StopSoundByEvent")]
	private string m_SoundStopEventName;

	public string SoundStartEventName => m_SoundStartEventName;

	public string SoundStopEventName => m_SoundStopEventName;

	public bool StopSoundByEvent => m_StopSoundByEvent;

	protected override bool StopPlaySignalIsReady(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		return SignalService.Instance.CheckReadyOrSend(ref commandData.StopPlaySignal);
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data data = player.GetCommandData<Data>(this);
		data.IsSkipped = true;
		data.Interchapter.Finished = true;
		EventBus.RaiseEvent(delegate(IInterchapterHandler h)
		{
			h.StopInterchapter(data.Interchapter);
		});
		CutsceneController.LockSkipBarkBanter.Release();
		Flag.Release();
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data data = player.GetCommandData<Data>(this);
		data.Interchapter = new InterchapterData();
		data.Interchapter.Finished = skipping;
		data.IsSkipped = skipping;
		if (!data.IsSkipped)
		{
			data.Interchapter.VideoClip = GetVideoToPlay();
			data.Interchapter.SoundStartEvent = m_SoundStartEventName;
			data.Interchapter.SoundStopEvent = (m_StopSoundByEvent ? m_SoundStopEventName : null);
			EventBus.RaiseEvent(delegate(IInterchapterHandler x)
			{
				x.StartInterchapter(data.Interchapter);
			});
			CutsceneController.LockSkipBarkBanter.Retain();
			Flag.Retain();
			data.StopPlaySignal = SignalService.Instance.RegisterNext();
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.Finished && !(time < commandData.Interchapter.VideoClip.length))
		{
			switch (commandData.Interchapter.State)
			{
			case null:
			case VideoState.Inactive:
			case VideoState.Finishing:
				commandData.Interchapter.Finished = true;
				break;
			case VideoState.Preparing:
				commandData.Interchapter.Finished = commandData.TimeSinceStateStart > MaxPreparingTime;
				break;
			case VideoState.Playing:
				commandData.Interchapter.Finished = commandData.TimeSinceStateStart > commandData.Interchapter.VideoClip.length + MaxPlayingLagTime;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case VideoState.PlayingPressAnyKey:
				break;
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data data = player.GetCommandData<Data>(this);
		if (!data.IsSkipped)
		{
			data.Interchapter.Finished = true;
			EventBus.RaiseEvent(delegate(IInterchapterHandler h)
			{
				h.StopInterchapter(data.Interchapter);
			});
			CutsceneController.LockSkipBarkBanter.Release();
			Flag.Release();
		}
	}

	public override string GetCaption()
	{
		return $"Play video FHD {m_VideoClip} / 4K {m_VideoClip4K}, {m_SoundStartEventName}, {m_SoundStopEventName}";
	}

	private VideoClip GetVideoToPlay()
	{
		if (!VideoHelper.IsCurrentResolutionMoreThanFHD)
		{
			return m_VideoClip;
		}
		if (m_VideoClip4K != null)
		{
			return m_VideoClip4K;
		}
		PFLog.Default.Warning("No 4K video clip, will play FHD");
		return m_VideoClip;
	}
}
