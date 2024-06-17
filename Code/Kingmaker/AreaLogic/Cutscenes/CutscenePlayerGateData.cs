using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutscenePlayerGateData : IHashable
{
	[JsonProperty]
	public Gate Gate;

	[JsonProperty]
	private bool m_IsActivated;

	[JsonProperty]
	public List<CutscenePlayerTrackData> IncomingTracks;

	[JsonProperty]
	public List<CutscenePlayerTrackData> StartedTracks;

	private bool m_PreventSignals;

	public CutscenePlayerData Player { get; set; }

	public bool Activated => m_IsActivated;

	public bool Signal()
	{
		if (Player == null)
		{
			return false;
		}
		if (m_PreventSignals)
		{
			return false;
		}
		if (Player.ActivatedGates.Contains(this))
		{
			return false;
		}
		switch (Gate ? Gate.Op : Operation.And)
		{
		case Operation.And:
			if (IncomingTracks.EmptyIfNull().All((CutscenePlayerTrackData t) => t.IsFinished || t.Track.IsContinuous))
			{
				Player.ActivateGate(this);
			}
			break;
		case Operation.Or:
			Player.ActivateGate(this);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return true;
	}

	public void Activate()
	{
		m_IsActivated = true;
		m_PreventSignals = true;
		if (IncomingTracks != null)
		{
			foreach (CutscenePlayerTrackData incomingTrack in IncomingTracks)
			{
				incomingTrack.ForceStop();
			}
		}
		m_PreventSignals = false;
		if (StartedTracks == null)
		{
			return;
		}
		foreach (CutscenePlayerTrackData startedTrack in StartedTracks)
		{
			startedTrack.Restart();
		}
	}

	public void StopExtraTracksOnStart()
	{
		List<CutscenePlayerTrackData> list = TempList.Get<CutscenePlayerTrackData>();
		foreach (CutscenePlayerTrackData startedTrack in StartedTracks)
		{
			if (startedTrack.Track.Commands.Count != 0 && (!startedTrack.Track.Commands[0].HasConditions || startedTrack.Track.Commands[0].EntryCondition.Check()))
			{
				list.Add(startedTrack);
			}
		}
		CutscenePlayerTrackData cutscenePlayerTrackData = null;
		if (list.Count > 0)
		{
			cutscenePlayerTrackData = ((Gate.ActivationMode == Gate.ActivationModeType.FirstTrack) ? list[0] : list[Player.Random.Range(0, list.Count)]);
		}
		foreach (CutscenePlayerTrackData startedTrack2 in StartedTracks)
		{
			if (startedTrack2 != cutscenePlayerTrackData)
			{
				startedTrack2.CommandIndex = startedTrack2.Track.Commands.Count;
				startedTrack2.IsPlaying = false;
				if (Gate.WhenTrackIsSkipped == Gate.SkipTracksModeType.SignalGate && startedTrack2.EndGate != null && !startedTrack2.Track.IsContinuous)
				{
					startedTrack2.EndGate.Signal();
				}
			}
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Gate);
		result.Append(ref val);
		result.Append(ref m_IsActivated);
		List<CutscenePlayerTrackData> incomingTracks = IncomingTracks;
		if (incomingTracks != null)
		{
			for (int i = 0; i < incomingTracks.Count; i++)
			{
				Hash128 val2 = ClassHasher<CutscenePlayerTrackData>.GetHash128(incomingTracks[i]);
				result.Append(ref val2);
			}
		}
		List<CutscenePlayerTrackData> startedTracks = StartedTracks;
		if (startedTracks != null)
		{
			for (int j = 0; j < startedTracks.Count; j++)
			{
				Hash128 val3 = ClassHasher<CutscenePlayerTrackData>.GetHash128(startedTracks[j]);
				result.Append(ref val3);
			}
		}
		return result;
	}
}
