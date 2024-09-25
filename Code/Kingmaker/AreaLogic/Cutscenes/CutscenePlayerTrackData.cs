using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutscenePlayerTrackData : IHashable
{
	[JsonProperty]
	public double PlayTime;

	[JsonProperty]
	public bool IsPlaying;

	[JsonProperty]
	public CutscenePlayerGateData StartGate;

	[JsonProperty]
	public CutscenePlayerGateData EndGate;

	[JsonProperty]
	public int TrackIndex;

	[JsonProperty]
	public int CommandIndex;

	[JsonProperty]
	public bool SignalSent;

	private bool m_Terminated;

	public Track Track => StartGate.Gate.StartedTracks[TrackIndex];

	public CommandBase Command
	{
		get
		{
			if (CommandIndex < 0 || CommandIndex >= Track.Commands.Count)
			{
				return null;
			}
			return Track.Commands[CommandIndex];
		}
	}

	public bool IsFinished => Track.Commands.Count <= CommandIndex;

	public bool IsRepeat => Track.Repeat;

	public void Tick(CutscenePlayerData player, [CanBeNull] out CutscenePlayerGateData signalReceiver, bool skipping)
	{
		signalReceiver = null;
		if (IsFinished)
		{
			return;
		}
		bool flag = !player.Cutscene.NonSkippable;
		if (IsPlaying)
		{
			PlayTime += Game.Instance.TimeController.DeltaTime;
			CommandBase commandBase = Track.Commands[CommandIndex];
			try
			{
				commandBase.SetTime(PlayTime, player);
				if ((!IsRepeat || (IsRepeat && flag)) && skipping && commandBase.TrySkip(player))
				{
					commandBase.Interrupt(player);
				}
				if (commandBase.TryPrepareForStop(player))
				{
					commandBase.Stop(player);
					IsPlaying = false;
				}
			}
			catch (Exception e)
			{
				IsPlaying = false;
				player.HandleException(e, this, commandBase);
			}
			if (!IsPlaying)
			{
				IAbstractUnitEntity controlledUnit = commandBase.GetControlledUnit();
				if (controlledUnit != null)
				{
					CutsceneControlledUnit.ReleaseUnit(controlledUnit, StartGate.Player);
				}
			}
		}
		if (!IsPlaying)
		{
			StartNextCommand(skipping, flag);
			if (IsFinished && !SignalSent)
			{
				SignalSent = true;
				signalReceiver = EndGate;
			}
		}
	}

	public void Restart()
	{
		CommandIndex = -1;
		if (SignalSent && EndGate.Gate != null && EndGate.Gate.Op == Operation.And && !EndGate.Activated)
		{
			SignalSent = false;
		}
	}

	private void StartNextCommand(bool skipping, bool isCutsceneSkippable)
	{
		CommandIndex++;
		if (IsFinished)
		{
			if (!IsRepeat || m_Terminated || (skipping && isCutsceneSkippable))
			{
				return;
			}
			CommandIndex = 0;
		}
		CommandBase commandBase = Track.Commands[CommandIndex];
		if (!commandBase.EntryCondition.Check())
		{
			switch (commandBase.OnFail)
			{
			case CommandBase.EntryFailResult.RemoveTrack:
				StartGate.StartedTracks.Remove(this);
				break;
			case CommandBase.EntryFailResult.FinishTrack:
				ForceStop();
				break;
			case CommandBase.EntryFailResult.SkipCommand:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		else
		{
			StartCommand(skipping);
		}
	}

	private bool StartCommand(bool skipping)
	{
		CommandBase command = Command;
		IAbstractUnitEntity controlledUnit = command.GetControlledUnit();
		if (controlledUnit != null && StartGate.Player.Paused)
		{
			CutsceneControlledUnit.MarkUnit(controlledUnit, StartGate.Player);
			IsPlaying = true;
			return false;
		}
		try
		{
			StartGate.Player.ClearCommandData(command);
			using (ProfileScope.New("Run Command"))
			{
				command.Run(StartGate.Player, skipping);
			}
			PlayTime = 0.0;
			IsPlaying = command.IsContinuous || !command.IsFinished(StartGate.Player);
			if (!IsPlaying)
			{
				command.Stop(StartGate.Player);
			}
		}
		catch (Exception e)
		{
			IsPlaying = false;
			StartGate.Player.HandleException(e, this, command);
		}
		if (controlledUnit != null && IsPlaying)
		{
			CutsceneControlledUnit.MarkUnit(controlledUnit, StartGate.Player);
		}
		return true;
	}

	public void ForceStop()
	{
		if (IsFinished)
		{
			return;
		}
		m_Terminated = true;
		CommandBase command = Command;
		try
		{
			if (command != null && !command.IsFinished(StartGate.Player))
			{
				command.Stop(StartGate.Player);
			}
		}
		catch (Exception e)
		{
			StartGate.Player.HandleException(e, this, command);
		}
		IAbstractUnitEntity abstractUnitEntity = (command ? command.GetControlledUnit() : null);
		if (abstractUnitEntity != null)
		{
			CutsceneControlledUnit.ReleaseUnit(abstractUnitEntity, StartGate.Player);
		}
		CommandIndex = Track.Commands.Count;
		IsPlaying = false;
		EndGate.Signal();
		if (StartGate != null && StartGate.StartedTracks.All((CutscenePlayerTrackData t) => t.IsFinished) && !StartGate.Activated)
		{
			StartGate.Activate();
		}
	}

	public void ForceGoToEndGate()
	{
		ForceStopStartedTracksBeforeGate(StartGate, EndGate);
	}

	private static void ForceStopStartedTracksBeforeGate(CutscenePlayerGateData gate, CutscenePlayerGateData endGate)
	{
		if (gate == endGate || gate?.StartedTracks == null)
		{
			return;
		}
		List<CutscenePlayerGateData> list = TempList.Get<CutscenePlayerGateData>();
		foreach (CutscenePlayerTrackData startedTrack in gate.StartedTracks)
		{
			if (!list.HasItem(startedTrack.EndGate))
			{
				list.Add(startedTrack.EndGate);
			}
			startedTrack.ForceStop();
		}
		foreach (CutscenePlayerGateData item in list)
		{
			ForceStopStartedTracksBeforeGate(item, endGate);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref PlayTime);
		result.Append(ref IsPlaying);
		Hash128 val = ClassHasher<CutscenePlayerGateData>.GetHash128(StartGate);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<CutscenePlayerGateData>.GetHash128(EndGate);
		result.Append(ref val2);
		result.Append(ref TrackIndex);
		result.Append(ref CommandIndex);
		result.Append(ref SignalSent);
		return result;
	}
}
