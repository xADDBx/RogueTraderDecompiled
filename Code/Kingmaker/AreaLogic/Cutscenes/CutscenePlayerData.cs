using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.AreaLogic.Cutscenes.Components;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutscenePlayerData : Entity, ICutscenePlayerData, IHashable
{
	private class EvaluationFailedHandlingFlag : ContextFlag<EvaluationFailedHandlingFlag>
	{
	}

	public static readonly LogChannel Logger = PFLog.Cutscene;

	private static CutscenePlayerData s_LastQueuedCutscene;

	[JsonProperty]
	private CutscenePlayerData m_QueuedAfter;

	[JsonProperty]
	private bool m_IsQueued;

	[JsonProperty]
	private readonly Cutscene m_Cutscene;

	[JsonProperty]
	private CutscenePlayerGateData m_CutsceneData;

	[JsonProperty]
	private readonly CutscenePlayerGateData m_ExitGate = new CutscenePlayerGateData();

	[JsonProperty]
	private readonly List<CutscenePlayerGateData> m_ActivatedGates = new List<CutscenePlayerGateData>();

	[JsonProperty]
	public NamedParametersContext Parameters = new NamedParametersContext();

	[JsonProperty]
	private bool m_Restart;

	[JsonProperty]
	private bool m_Remove;

	[JsonProperty]
	private readonly List<EntityFactRef> m_HiddenPetFacts = new List<EntityFactRef>();

	private bool m_RestoreCalled;

	private bool m_StoppingInProgress;

	private Dictionary<Gate, CutscenePlayerGateData> m_GateData;

	private readonly Dictionary<CommandBase, object> m_CommandData = new Dictionary<CommandBase, object>();

	private readonly Dictionary<CutscenePauseReason, int> m_Paused = new Dictionary<CutscenePauseReason, int>();

	private readonly Dictionary<CutscenePauseReason, int> m_PauseDelayed = new Dictionary<CutscenePauseReason, int>();

	private bool m_PausedSleeping;

	private bool m_TickInProgress;

	private bool m_AnchorsCollected;

	private bool m_ResourcesCollected;

	public readonly List<EntityRef> Anchors = new List<EntityRef>();

	public readonly HashSet<CommandBase> FailedCommands = new HashSet<CommandBase>();

	public readonly HashSet<CommandBase> FailedCheckCommands = new HashSet<CommandBase>();

	public readonly HashSet<Track> FinishedTracks = new HashSet<Track>();

	public SimpleBlueprint OriginBlueprint;

	[JsonProperty]
	private StatefulRandom m_Random;

	public List<LogInfo> LogList = new List<LogInfo>();

	public CutscenePauseReason LastHandledReason;

	private List<CutscenePlayerGateData> m_FastGates = new List<CutscenePlayerGateData>();

	[JsonProperty]
	public bool Exclusive { get; private set; }

	[JsonProperty]
	public string PlayActionId { get; set; }

	[JsonProperty]
	public bool IsFinished { get; private set; }

	public bool IsStoppingInProgress => m_StoppingInProgress;

	public bool PreventDestruction { get; set; }

	public bool TraceCommands { get; set; }

	public ParametrizedContextSetter ParameterSetter { get; set; }

	public bool Paused => m_Paused.Count > 0;

	public new CutscenePlayerView View => (CutscenePlayerView)base.View;

	public List<CutscenePlayerGateData> ActivatedGates => m_ActivatedGates;

	public bool IsFrozen { get; private set; }

	public bool ShouldBeFreezed
	{
		get
		{
			if (!Cutscene.Freezeless && Anchors.Count > 0)
			{
				return CheckIsShouldBeFreezed();
			}
			return false;
		}
	}

	public bool AllAnchorsInactive
	{
		get
		{
			if (Anchors.Count > 0)
			{
				return !CheckIsAnyAnchorActive();
			}
			return false;
		}
	}

	public Cutscene Cutscene => m_Cutscene;

	public ICutscene ICutscene => Cutscene;

	public bool IsQueued => m_IsQueued;

	public bool IsFirstInQueue
	{
		get
		{
			if (m_IsQueued)
			{
				if (m_QueuedAfter != null)
				{
					return m_QueuedAfter.IsDisposed;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsLastInQueue
	{
		get
		{
			if (m_IsQueued)
			{
				return s_LastQueuedCutscene == this;
			}
			return false;
		}
	}

	public IEnumerable<CutscenePauseReason> PauseReasons => from i in m_Paused
		where i.Value > 0
		select i.Key;

	public static IEnumerable<CutscenePlayerData> Queue
	{
		get
		{
			for (CutscenePlayerData q = s_LastQueuedCutscene; q != null; q = q.m_QueuedAfter)
			{
				yield return q;
			}
		}
	}

	public StatefulRandom Random => m_Random;

	public CutscenePlayerData(Cutscene cutscene, ICutscenePlayerView player)
		: base(player.UniqueViewId, player.IsInGameBySettings)
	{
		m_Cutscene = cutscene;
		m_CutsceneData = new CutscenePlayerGateData
		{
			Gate = m_Cutscene
		};
		m_GateData = new Dictionary<Gate, CutscenePlayerGateData>();
		FillGateData(m_CutsceneData);
		m_RestoreCalled = true;
		if (Cutscene.IsBackground && Cutscene.LockControl)
		{
			LogError("Cutscene " + Cutscene.NameSafe() + " is background, but also locks control. This is a bug.");
			player.Cutscene.IsBackground = false;
		}
	}

	protected CutscenePlayerData(JsonConstructorMark _)
		: base(_)
	{
	}

	public CutscenePlayerDataScope GetDataScope()
	{
		return ContextData<CutscenePlayerDataScope>.Request().Setup(this);
	}

	public T GetCommandData<T>(CommandBase cmd) where T : class, new()
	{
		m_CommandData.TryGetValue(cmd, out var value);
		T val = value as T;
		if (val == null)
		{
			val = (T)(m_CommandData[cmd] = new T());
		}
		return val;
	}

	public void ClearCommandData(CommandBase cmd)
	{
		m_CommandData.Remove(cmd);
	}

	public void SetPaused(bool value, CutscenePauseReason reason)
	{
		if (m_TickInProgress)
		{
			m_PauseDelayed[reason] = m_PauseDelayed.Get(reason, 0) + (value ? 1 : (-1));
			return;
		}
		bool paused = Paused;
		int num = m_Paused.Get(reason, 0) + (value ? 1 : (-1));
		if (num <= 0)
		{
			m_Paused.Remove(reason);
		}
		else
		{
			m_Paused[reason] = num;
		}
		if (Paused && !paused)
		{
			using (Parameters.RequestContextData())
			{
				foreach (CutscenePlayerGateData activatedGate in m_ActivatedGates)
				{
					foreach (CutscenePlayerTrackData startedTrack in activatedGate.StartedTracks)
					{
						try
						{
							if (startedTrack.IsPlaying && (bool)startedTrack.Command)
							{
								startedTrack.Command.Stop(this);
							}
						}
						catch (Exception e)
						{
							HandleException(e, startedTrack, startedTrack.Command);
						}
					}
				}
			}
			RaiseEvent(this, delegate(ICutsceneHandler h)
			{
				h.HandleCutscenePaused(reason);
			});
			LastHandledReason = reason;
		}
		if (paused && !Paused)
		{
			Restore(markControlledObjects: false);
			RaiseEvent(this, delegate(ICutsceneHandler h)
			{
				h.HandleCutsceneResumed();
			});
		}
	}

	public void InterruptBark()
	{
		if (Paused)
		{
			return;
		}
		using (Parameters.RequestContextData())
		{
			if (TryInterruptCommandByType<CommandBarkUnit>() || TryInterruptCommandByType<CommandBarkEntity>())
			{
				TryInterruptCommandByType<CommandUnitPlayCutsceneAnimation>();
			}
		}
	}

	private bool TryInterruptCommandByType<T>()
	{
		foreach (CutscenePlayerGateData activatedGate in m_ActivatedGates)
		{
			foreach (CutscenePlayerTrackData startedTrack in activatedGate.StartedTracks)
			{
				try
				{
					if (startedTrack.IsPlaying && (bool)startedTrack.Command && startedTrack.Command is T)
					{
						startedTrack.Command.Interrupt(this);
						return true;
					}
				}
				catch (Exception e)
				{
					HandleException(e, startedTrack, startedTrack.Command);
				}
			}
		}
		return false;
	}

	private bool CheckIsShouldBeFreezed()
	{
		if (Cutscene.LockControl || Cutscene.Sleepless)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = true;
		foreach (EntityRef anchor in Anchors)
		{
			IEntity entity = anchor.Entity;
			if (entity != null && entity.IsInGame && entity is AbstractUnitEntity { FreezeOutsideCamera: not false } abstractUnitEntity)
			{
				flag = true;
				if (!abstractUnitEntity.IsSleeping)
				{
					flag2 = false;
					break;
				}
			}
		}
		return flag && flag2;
	}

	private bool CheckIsAnyAnchorActive()
	{
		if (Cutscene.Sleepless)
		{
			return true;
		}
		bool flag = false;
		foreach (EntityRef anchor in Anchors)
		{
			IEntity entity = anchor.Entity;
			if (entity == null || !entity.IsInGame)
			{
				continue;
			}
			if (entity.Suppressed)
			{
				return false;
			}
			if (!entity.IsInFogOfWar)
			{
				return true;
			}
			Vector3 position = entity.Position;
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if ((partyAndPet.Position - position).sqrMagnitude <= 585.64f)
				{
					return true;
				}
			}
			flag = true;
		}
		return !flag;
	}

	private void FillGateData(CutscenePlayerGateData gate, bool logCreate = false)
	{
		gate.Player = this;
		gate.StartedTracks = (gate.Gate ? gate.Gate.StartedTracks.Select((Track t, int i) => CutscenePlayerTrackData.Create(gate, i)).ToList() : new List<CutscenePlayerTrackData>());
		m_GateData[gate.Gate] = gate;
		foreach (CutscenePlayerTrackData startedTrack in gate.StartedTracks)
		{
			startedTrack.Track.Commands.RemoveAll((CommandBase c) => !c);
			startedTrack.EndGate = (startedTrack.Track.EndGate ? GetOrCreateAndFillGateData(startedTrack.Track.EndGate, logCreate) : m_ExitGate);
			startedTrack.EndGate.IncomingTracks = startedTrack.EndGate.IncomingTracks ?? new List<CutscenePlayerTrackData>();
			startedTrack.EndGate.IncomingTracks.Add(startedTrack);
			foreach (CommandBase command in startedTrack.Track.Commands)
			{
				CommandBase.CommandSignalData[] array = command.GetExtraSignals().EmptyIfNull();
				foreach (CommandBase.CommandSignalData commandSignalData in array)
				{
					if (commandSignalData?.Gate != null)
					{
						GetOrCreateAndFillGateData(commandSignalData.Gate, logCreate);
					}
				}
			}
		}
	}

	private void RestoreGateData(CutscenePlayerGateData gate)
	{
		m_GateData[gate.Gate] = gate;
		gate.Player = this;
		foreach (CutscenePlayerTrackData startedTrack in gate.StartedTracks)
		{
			if (startedTrack.EndGate?.Gate != null && !m_GateData.ContainsKey(startedTrack.EndGate.Gate))
			{
				RestoreGateData(startedTrack.EndGate);
			}
		}
	}

	private CutscenePlayerGateData GetOrCreateAndFillGateData(Gate gate, bool logCreate = false)
	{
		if (!m_GateData.TryGetValue(gate, out var value))
		{
			if (logCreate)
			{
				LogError($"Cutscene {Cutscene} was missing Gate Data for {gate}, adding.");
			}
			value = new CutscenePlayerGateData
			{
				Gate = gate
			};
			FillGateData(value, logCreate);
		}
		return value;
	}

	protected override IEntityViewBase CreateViewForData()
	{
		if (!Cutscene)
		{
			return null;
		}
		CutscenePlayerView cutscenePlayerView = new GameObject("[cutscene player " + Cutscene.name + "]").AddComponent<CutscenePlayerView>();
		cutscenePlayerView.Cutscene = Cutscene;
		cutscenePlayerView.UniqueId = base.UniqueId;
		return cutscenePlayerView;
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		uint uintValue = PFStatefulRandom.Cutscene.uintValue;
		if (m_Random == null)
		{
			m_Random = new StatefulRandom("Cutscene " + base.UniqueId, uintValue);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (Cutscene != null && Cutscene.GetComponent<DestroyCutsceneOnLoad>() != null)
		{
			MarkRemoved();
		}
		Cutscene cutscene = Cutscene;
		if (cutscene != null && cutscene.LockControl && !Game.Instance.IsLoadingFromSave)
		{
			PFLog.Cutscene.Error("Removing unexpected lock-control cutscene " + Cutscene.Name + " during post-load");
			MarkRemoved();
		}
		foreach (CutscenePlayerGateData activatedGate in m_ActivatedGates)
		{
			SetPlayer(activatedGate);
		}
		if (m_IsQueued && (s_LastQueuedCutscene == null || s_LastQueuedCutscene.HoldingState != HoldingState))
		{
			FixupMultipleQueues();
		}
		StopCopies();
	}

	private void FixupMultipleQueues()
	{
		s_LastQueuedCutscene = null;
		foreach (CutscenePlayerData cutscene in Game.Instance.State.Cutscenes)
		{
			if (!cutscene.m_IsQueued)
			{
				continue;
			}
			s_LastQueuedCutscene = s_LastQueuedCutscene ?? cutscene;
			CutscenePlayerData queuedAfter = s_LastQueuedCutscene;
			while (queuedAfter != null && queuedAfter != cutscene)
			{
				queuedAfter = queuedAfter.m_QueuedAfter;
			}
			if (queuedAfter != cutscene)
			{
				queuedAfter = cutscene;
				while (queuedAfter.m_QueuedAfter != null && queuedAfter.m_QueuedAfter != s_LastQueuedCutscene)
				{
					queuedAfter = queuedAfter.m_QueuedAfter;
				}
				if (queuedAfter.m_QueuedAfter == s_LastQueuedCutscene)
				{
					s_LastQueuedCutscene = cutscene;
					continue;
				}
				queuedAfter.m_QueuedAfter = s_LastQueuedCutscene;
				s_LastQueuedCutscene = cutscene;
			}
		}
	}

	private void SetPlayer(CutscenePlayerGateData gate)
	{
		if (gate.Player == this || gate == m_ExitGate)
		{
			return;
		}
		gate.Player = this;
		foreach (CutscenePlayerTrackData startedTrack in gate.StartedTracks)
		{
			SetPlayer(startedTrack.EndGate);
		}
	}

	private bool TickGate(CutscenePlayerGateData gateData, List<CutscenePlayerGateData> signalReceivers, bool skipping)
	{
		bool flag = true;
		for (int i = 0; i < gateData.StartedTracks.Count; i++)
		{
			CutscenePlayerTrackData cutscenePlayerTrackData = gateData.StartedTracks[i];
			try
			{
				cutscenePlayerTrackData.Tick(this, out var signalReceiver, skipping);
				flag = flag && cutscenePlayerTrackData.IsFinished;
				if (signalReceiver != null)
				{
					signalReceivers.Add(signalReceiver);
				}
			}
			catch (Exception e)
			{
				gateData.StartedTracks.RemoveAt(i);
				i--;
				HandleException(e, cutscenePlayerTrackData, null);
			}
		}
		return flag;
	}

	private void DoSignalReceivers(List<CutscenePlayerGateData> signalReceivers)
	{
		foreach (CutscenePlayerGateData signalReceiver in signalReceivers)
		{
			try
			{
				signalReceiver.Signal();
			}
			catch (Exception e)
			{
				HandleException(e, null, null);
			}
		}
	}

	public void TickScene(bool skipping = false)
	{
		SceneEntitiesState holdingState = HoldingState;
		if (holdingState != null && !holdingState.IsSceneLoaded)
		{
			Game.Instance.EntityDestroyer.Destroy(this);
			PFLog.Cutscene.Error("[Cutscene] Remove invalid cutscene " + Cutscene.Name + ", owner state " + HoldingState.SceneName + " don`t load!");
			return;
		}
		if (IsFinished)
		{
			if (!PreventDestruction)
			{
				Game.Instance.EntityDestroyer.Destroy(this);
			}
			return;
		}
		if (m_QueuedAfter != null)
		{
			if (!m_QueuedAfter.IsInState)
			{
				m_QueuedAfter = null;
			}
			else if (!m_QueuedAfter.IsFinished)
			{
				return;
			}
		}
		try
		{
			CollectAnchors();
			if (!m_RestoreCalled)
			{
				Restore();
				if (IsFinished)
				{
					return;
				}
			}
			using (ProfileScope.New("UpdateActiveCutscenes"))
			{
				foreach (EntityRef anchor in Anchors)
				{
					if (anchor.Entity is BaseUnitEntity unit)
					{
						CutsceneControlledUnit.UpdateActiveCutscene(unit);
					}
				}
			}
			bool pausedSleeping = m_PausedSleeping;
			using (ProfileScope.New("Check Should Pause Cutscene"))
			{
				m_PausedSleeping = !Cutscene.Sleepless && !Cutscene.LockControl && AllAnchorsInactive;
			}
			if (m_PausedSleeping != pausedSleeping)
			{
				SetPaused(m_PausedSleeping, CutscenePauseReason.HasNoActiveAnchors);
			}
			if (Paused)
			{
				return;
			}
			IsFrozen = ShouldBeFreezed;
			if (IsFrozen)
			{
				return;
			}
			m_TickInProgress = true;
			m_PauseDelayed.Clear();
			Parameters.Cutscene = this;
			using (Parameters.RequestContextData())
			{
				List<CutscenePlayerGateData> list = TempList.Get<CutscenePlayerGateData>();
				for (int i = 0; i < m_ActivatedGates.Count; i++)
				{
					if (TickGate(m_ActivatedGates[i], list, skipping))
					{
						m_ActivatedGates.RemoveAt(i);
						i--;
					}
				}
				DoSignalReceivers(list);
				while (m_FastGates.Count > 0)
				{
					list.Clear();
					List<CutscenePlayerGateData> list2 = m_FastGates.ToTempList();
					m_FastGates.Clear();
					foreach (CutscenePlayerGateData item in list2)
					{
						if (TickGate(item, list, skipping))
						{
							m_ActivatedGates.Remove(item);
						}
					}
					DoSignalReceivers(list);
				}
			}
			m_TickInProgress = false;
			foreach (KeyValuePair<CutscenePauseReason, int> item2 in m_PauseDelayed)
			{
				var (reason, j) = (KeyValuePair<CutscenePauseReason, int>)(ref item2);
				while (j > 0)
				{
					SetPaused(value: true, reason);
					j--;
				}
				for (; j < 0; j++)
				{
					SetPaused(value: false, reason);
				}
			}
			if (m_ActivatedGates.Count == 0)
			{
				IsFinished = true;
				using (Parameters.RequestContextData())
				{
					Cutscene.OnStopped.Run();
				}
			}
		}
		catch (Exception e)
		{
			m_ActivatedGates.Clear();
			IsFinished = true;
			HandleException(e, null, null);
		}
		finally
		{
			m_TickInProgress = false;
		}
		if (IsFinished)
		{
			Cutscene.OnFinished?.Run();
			if (Cutscene.LockControl && !Cutscene.ShowPets)
			{
				TryShowPets();
			}
		}
		if (IsFinished && !PreventDestruction)
		{
			Game.Instance.EntityDestroyer.Destroy(this);
		}
	}

	public void Start(bool queued = false)
	{
		ParametrizedContextSetter.ParameterEntry[] parameters = Cutscene.DefaultParameters.Parameters;
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in parameters)
		{
			if (!Parameters.Params.ContainsKey(parameterEntry.Name))
			{
				Parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
			}
		}
		StopCopies();
		using (Parameters.RequestContextData())
		{
			ActivateGate(m_CutsceneData);
		}
		PreloadCutsceneResources();
		if (queued)
		{
			m_IsQueued = true;
			if (s_LastQueuedCutscene != null && !s_LastQueuedCutscene.IsDisposed)
			{
				m_QueuedAfter = s_LastQueuedCutscene;
			}
			s_LastQueuedCutscene = this;
		}
		RaiseEvent(this, delegate(ICutsceneHandler h)
		{
			h.HandleCutsceneStarted(queued);
		});
		if (Cutscene.LockControl && !Cutscene.ShowPets)
		{
			TryHidePets();
		}
	}

	private void CollectResourcesFromAction(GameAction action, HashSet<EntityRef> result, HashSet<Gate> seenGates)
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			if (action is Spawn { Spawners: var spawners } spawn)
			{
				for (int i = 0; i < spawners.Length; i++)
				{
					UnitSpawnerBase unitSpawner = GameHelper.GetUnitSpawner(spawners[i]);
					if (!(unitSpawner?.Blueprint?.Prefab == null))
					{
						unitSpawner.Blueprint.Prefab.Load();
						GameAction[] actions = spawn.ActionsOnSpawn.Actions;
						foreach (GameAction action2 in actions)
						{
							CollectResourcesFromAction(action2, result, seenGates);
						}
					}
				}
			}
			else if (action is SpawnFx spawnFx)
			{
				PooledGameObject component = spawnFx.FxPrefab.GetComponent<PooledGameObject>();
				if (component != null)
				{
					GameObjectsPool.Warmup(component, 1);
				}
			}
			else if (action is AttachBuff attachBuff)
			{
				attachBuff.Buff.FxOnStart?.Load();
				attachBuff.Buff.FxOnRemove?.Load();
			}
			else if (action is PlayCutscene playCutscene)
			{
				CollectResourcesFromGate(playCutscene.Cutscene, result, seenGates);
			}
			else if (action is Conditional conditional)
			{
				GameAction[] actions = conditional.IfTrue.Actions;
				foreach (GameAction action3 in actions)
				{
					CollectResourcesFromAction(action3, result, seenGates);
				}
				actions = conditional.IfFalse.Actions;
				foreach (GameAction action4 in actions)
				{
					CollectResourcesFromAction(action4, result, seenGates);
				}
			}
		}
	}

	private void CollectResourcesFromGate(Gate gate, HashSet<EntityRef> result, HashSet<Gate> seenGates)
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			if (!seenGates.Add(gate))
			{
				return;
			}
			foreach (Track startedTrack in gate.StartedTracks)
			{
				if (startedTrack == null)
				{
					continue;
				}
				foreach (CommandBase command in startedTrack.Commands)
				{
					if (command == null)
					{
						continue;
					}
					if (command is CommandAction commandAction && commandAction.Action?.Actions != null)
					{
						GameAction[] actions = commandAction.Action.Actions;
						foreach (GameAction action in actions)
						{
							CollectResourcesFromAction(action, result, seenGates);
						}
					}
					else if (command is CommandUnitPlayCutsceneAnimation commandUnitPlayCutsceneAnimation)
					{
						commandUnitPlayCutsceneAnimation.Preload();
					}
				}
				if (startedTrack.EndGate != null)
				{
					CollectResourcesFromGate(startedTrack.EndGate, result, seenGates);
				}
			}
		}
	}

	public void PreloadCutsceneResources()
	{
		if (m_ResourcesCollected)
		{
			return;
		}
		m_ResourcesCollected = true;
		using (CodeTimer.New($"PreloadCutsceneResources {View}"))
		{
			using (ProfileScope.New("PreloadCutsceneResources"))
			{
				HashSet<Gate> seenGates = TempHashSet.Get<Gate>();
				CollectResourcesFromGate(m_CutsceneData.Gate, null, seenGates);
			}
		}
	}

	private void StopCopies()
	{
		if (Cutscene.AllowCopies || m_Remove)
		{
			return;
		}
		foreach (CutscenePlayerData item in Game.Instance.State.Cutscenes.ToTempList())
		{
			if (item != this && item.Cutscene == Cutscene && Parameters.IsTheSame(item.Parameters))
			{
				item.Stop();
				item.m_Remove = true;
			}
		}
	}

	public void ReactivateGate(Gate gate)
	{
		if (m_GateData == null)
		{
			m_GateData = new Dictionary<Gate, CutscenePlayerGateData>();
			RestoreGateData(m_CutsceneData);
		}
		if (!m_GateData.TryGetValue(gate, out var value))
		{
			LogError($"{Cutscene} cannot reactivate gate {gate}: not found in current cutscene", needQaReport: true);
		}
		else if (!value.Activated)
		{
			LogError($"{Cutscene} cannot activate gate {gate}: never activated before", needQaReport: true);
		}
		else if (m_ActivatedGates.Contains(value))
		{
			LogError($"{Cutscene} cannot activate gate {gate}: still has playing tracks", needQaReport: true);
		}
		else
		{
			ActivateGate(value);
		}
	}

	public void SignalGate(Gate gate)
	{
		if (m_GateData == null)
		{
			m_GateData = new Dictionary<Gate, CutscenePlayerGateData>();
			RestoreGateData(m_CutsceneData);
		}
		if (!m_GateData.TryGetValue(gate, out var value))
		{
			LogError($"{Cutscene} cannot signal gate {gate}: not found in current cutscene");
		}
		else
		{
			value.Signal();
		}
	}

	public void ActivateGate(CutscenePlayerGateData gate)
	{
		if (!gate.Gate.PauseForOneFrame)
		{
			m_FastGates.Add(gate);
		}
		gate.Player = this;
		m_ActivatedGates.Add(gate);
		gate.Activate();
		if (gate.IncomingTracks != null)
		{
			foreach (CutscenePlayerTrackData incomingTrack in gate.IncomingTracks)
			{
				if (incomingTrack.IsFinished)
				{
					incomingTrack.SignalSent = false;
				}
			}
		}
		if (gate.Gate.ActivationMode != 0)
		{
			gate.StopExtraTracksOnStart();
		}
	}

	protected override void OnDispose()
	{
		using (Parameters.RequestContextData())
		{
			foreach (CutscenePlayerGateData activatedGate in m_ActivatedGates)
			{
				foreach (CutscenePlayerTrackData startedTrack in activatedGate.StartedTracks)
				{
					if (startedTrack.IsPlaying && (bool)startedTrack.Command)
					{
						startedTrack.Command.GetControlledUnit()?.ToAbstractUnitEntity().CutsceneControlledUnit?.Release(this);
						try
						{
							startedTrack.Command.Stop(this);
						}
						catch (Exception e)
						{
							HandleException(e, startedTrack, startedTrack.Command);
						}
					}
				}
			}
		}
		if (s_LastQueuedCutscene == this)
		{
			s_LastQueuedCutscene = null;
		}
		base.OnDispose();
	}

	private void Restore(bool markControlledObjects = true)
	{
		m_RestoreCalled = true;
		if (!m_Remove)
		{
			SceneEntitiesState holdingState = HoldingState;
			if (holdingState == null || holdingState.IsSceneLoaded)
			{
				if (m_Restart)
				{
					m_Remove = false;
					m_Restart = false;
					m_ActivatedGates.Clear();
					Exclusive = false;
					m_CutsceneData = new CutscenePlayerGateData
					{
						Gate = m_Cutscene
					};
					m_GateData = new Dictionary<Gate, CutscenePlayerGateData>();
					FillGateData(m_CutsceneData);
					ActivateGate(m_CutsceneData);
					RaiseEvent(this, delegate(ICutsceneHandler h)
					{
						h.HandleCutsceneRestarted();
					});
					return;
				}
				m_GateData = new Dictionary<Gate, CutscenePlayerGateData>();
				RestoreGateData(m_CutsceneData);
				foreach (CutscenePlayerGateData item in PooledList<CutscenePlayerGateData>.Get(m_GateData.Values))
				{
					RemoveInvalidAndAddMissingTracks(item);
					FixInvalidGateConnections(item);
				}
				using (Parameters.RequestContextData())
				{
					foreach (CutscenePlayerGateData activatedGate in m_ActivatedGates)
					{
						foreach (CutscenePlayerTrackData startedTrack in activatedGate.StartedTracks)
						{
							if (!startedTrack.IsPlaying || startedTrack.IsFinished)
							{
								continue;
							}
							CommandBase command = startedTrack.Command;
							if (markControlledObjects)
							{
								IAbstractUnitEntity controlledUnit = command.GetControlledUnit();
								if (controlledUnit != null && !CutsceneControlledUnit.MarkUnit(controlledUnit, this))
								{
									LogError($"Cannot restore cutscene {Cutscene} as another cutscene ({controlledUnit.ToAbstractUnitEntity().CutsceneControlledUnit?.GetCurrentlyActive()}) controls an object ({controlledUnit}) ({command})");
								}
							}
							if (!Paused)
							{
								try
								{
									command.Run(this);
								}
								catch (Exception e)
								{
									HandleException(e, startedTrack, command);
								}
							}
						}
					}
					return;
				}
			}
		}
		m_Remove = false;
		m_Restart = false;
		m_IsQueued = false;
		m_QueuedAfter = null;
		m_ActivatedGates.Clear();
		IsFinished = true;
	}

	private void RemoveInvalidAndAddMissingTracks(CutscenePlayerGateData gate)
	{
		if (gate.StartedTracks == null || gate.Gate?.StartedTracks == null)
		{
			return;
		}
		List<CutscenePlayerTrackData> list = null;
		using PooledHashSet<int> pooledHashSet = PooledHashSet<int>.Get();
		foreach (CutscenePlayerTrackData startedTrack in gate.StartedTracks)
		{
			if (startedTrack.TrackIndex >= gate.Gate.StartedTracks.Count)
			{
				if (list == null)
				{
					list = ListPool<CutscenePlayerTrackData>.Claim();
				}
				LogError($"Cutscene {Cutscene} has saved track from gate {gate.Gate} with invalid index {startedTrack.TrackIndex}, removing.");
				list.Add(startedTrack);
			}
			else
			{
				pooledHashSet.Add(startedTrack.TrackIndex);
			}
		}
		if (list != null)
		{
			foreach (CutscenePlayerTrackData item in list)
			{
				gate.StartedTracks.Remove(item);
			}
			ListPool<CutscenePlayerTrackData>.Release(list);
			list = null;
		}
		for (int i = 0; i < gate.Gate.StartedTracks.Count; i++)
		{
			if (!pooledHashSet.Contains(i))
			{
				LogError($"Cutscene {Cutscene} is missing track #{i} on gate {gate.Gate}, adding.");
				gate.StartedTracks.Add(CutscenePlayerTrackData.Create(gate, i));
			}
		}
		if (gate.IncomingTracks == null)
		{
			return;
		}
		foreach (CutscenePlayerTrackData incomingTrack in gate.IncomingTracks)
		{
			if (incomingTrack.TrackIndex >= incomingTrack.StartGate.Gate.StartedTracks.Count)
			{
				if (list == null)
				{
					list = ListPool<CutscenePlayerTrackData>.Claim();
				}
				LogError($"Cutscene {Cutscene} has saved incoming track to gate {gate.Gate} with invalid index {incomingTrack.TrackIndex}, removing.");
				list.Add(incomingTrack);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (CutscenePlayerTrackData item2 in list)
		{
			gate.IncomingTracks.Remove(item2);
		}
		ListPool<CutscenePlayerTrackData>.Release(list);
	}

	private void FixInvalidGateConnections(CutscenePlayerGateData gate)
	{
		if (gate.StartedTracks == null)
		{
			return;
		}
		foreach (CutscenePlayerTrackData startedTrack in gate.StartedTracks)
		{
			if (startedTrack.Track.EndGate != null && (startedTrack.EndGate == null || startedTrack.EndGate.Gate != startedTrack.Track.EndGate))
			{
				CutscenePlayerGateData orCreateAndFillGateData = GetOrCreateAndFillGateData(startedTrack.Track.EndGate, logCreate: true);
				LogError($"Cutscene {Cutscene}, gate {gate.Gate} track #{startedTrack.TrackIndex} has incorrect EndGate, fixing.");
				startedTrack.EndGate?.IncomingTracks.Remove(startedTrack);
				startedTrack.EndGate = orCreateAndFillGateData;
				orCreateAndFillGateData.IncomingTracks.Add(startedTrack);
			}
		}
	}

	public bool IsGateActive(Gate gate)
	{
		return m_ActivatedGates.Any((CutscenePlayerGateData gd) => gd.Gate == gate);
	}

	public CutscenePlayerTrackData GetTrackData(Track track)
	{
		return m_ActivatedGates.SelectMany((CutscenePlayerGateData gd) => gd.StartedTracks).FirstOrDefault((CutscenePlayerTrackData t) => t.Track == track);
	}

	public void Stop()
	{
		if (m_StoppingInProgress)
		{
			return;
		}
		m_StoppingInProgress = true;
		if (m_RestoreCalled && !IsFinished)
		{
			foreach (CutscenePlayerGateData activatedGate in m_ActivatedGates)
			{
				foreach (CutscenePlayerTrackData startedTrack in activatedGate.StartedTracks)
				{
					if (!startedTrack.IsPlaying || startedTrack.IsFinished)
					{
						continue;
					}
					using (Parameters.RequestContextData())
					{
						CommandBase command = startedTrack.Command;
						try
						{
							command.Stop(this);
						}
						catch (Exception e)
						{
							HandleException(e, startedTrack, command);
						}
						command.GetControlledUnit()?.ToAbstractUnitEntity().CutsceneControlledUnit?.Release(this);
					}
				}
			}
			if (ContextData<SceneEntitiesState.DisposeInProgress>.Current == null)
			{
				using (Parameters.RequestContextData())
				{
					Cutscene.OnStopped.Run();
				}
			}
		}
		m_ActivatedGates.Clear();
		RaiseEvent(this, delegate(ICutsceneHandler h)
		{
			h.HandleCutsceneStopped();
		});
		if (Cutscene.LockControl && !Cutscene.ShowPets)
		{
			TryShowPets();
		}
		IsFinished = true;
		m_StoppingInProgress = false;
		if (!PreventDestruction)
		{
			Game.Instance.EntityDestroyer.Destroy(this);
		}
	}

	protected void MarkRemoved()
	{
		m_Remove = true;
	}

	public bool HasCommandData(CommandBase commandBase)
	{
		return m_CommandData.ContainsKey(commandBase);
	}

	private void CollectAnchors()
	{
		if (m_AnchorsCollected)
		{
			return;
		}
		Anchors.AddRange(Cutscene.Anchors.Select((EntityReference r) => new EntityRef(r.UniqueId)));
		m_AnchorsCollected = true;
		using (Parameters.RequestContextData())
		{
			HashSet<EntityRef> hashSet = new HashSet<EntityRef>();
			HashSet<Gate> seenGates = new HashSet<Gate>();
			CollectAnchorsFromGate(Cutscene, hashSet, seenGates);
			Anchors.AddRange(hashSet);
		}
	}

	public void CollectAnchorsFromGate(Gate gate, HashSet<EntityRef> result, HashSet<Gate> seenGates)
	{
		if (!seenGates.Add(gate))
		{
			return;
		}
		foreach (Track startedTrack in gate.StartedTracks)
		{
			if (startedTrack == null)
			{
				continue;
			}
			foreach (CommandBase command in startedTrack.Commands)
			{
				if (command != null)
				{
					AbstractUnitEntity abstractUnitEntity = null;
					try
					{
						abstractUnitEntity = command.GetAnchorUnit().ToAbstractUnitEntity();
					}
					catch (Exception e)
					{
						HandleException(e, null, command);
					}
					if (abstractUnitEntity != null)
					{
						result.Add(abstractUnitEntity.Ref);
					}
				}
			}
			if (startedTrack.EndGate != null)
			{
				CollectAnchorsFromGate(startedTrack.EndGate, result, seenGates);
			}
		}
	}

	public List<AbstractUnitEntity> GetCurrentControlledUnits()
	{
		List<AbstractUnitEntity> list = new List<AbstractUnitEntity>();
		foreach (CutscenePlayerGateData activatedGate in m_ActivatedGates)
		{
			foreach (CutscenePlayerTrackData startedTrack in activatedGate.StartedTracks)
			{
				if (!startedTrack.IsPlaying || startedTrack.IsFinished)
				{
					continue;
				}
				using (Parameters.RequestContextData())
				{
					CommandBase command = startedTrack.Command;
					try
					{
						IAbstractUnitEntity controlledUnit = command.GetControlledUnit();
						if (controlledUnit != null)
						{
							list.Add(controlledUnit.ToAbstractUnitEntity());
						}
					}
					catch (Exception e)
					{
						HandleException(e, startedTrack, command);
					}
				}
			}
		}
		return list;
	}

	public void LogError(string message, bool needQaReport = false, [CanBeNull] CommandBase failedCommand = null)
	{
		CutsceneLogSink.Instance.PrepareForLog(this, failedCommand);
		if (needQaReport)
		{
			Logger.ErrorWithReport(Cutscene, message);
		}
		else
		{
			Logger.Error(Cutscene, message);
		}
	}

	public void LogCommandTrace(string message)
	{
		if (TraceCommands)
		{
			LogSeverity minStackTraceLevel = Logger.MinStackTraceLevel;
			Logger.SetMinStackTraceLevel(LogSeverity.Error);
			CutsceneLogSink.Instance.PrepareForLog(this, null);
			Logger.Log(Cutscene, $"[{Cutscene}] {message}");
			Logger.SetMinStackTraceLevel(minStackTraceLevel);
		}
	}

	public void HandleException(Exception e, [CanBeNull] CutscenePlayerTrackData track, [CanBeNull] CommandBase command)
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.AppendFormat("Exception in cutscene {0} (", Cutscene.NameSafe());
		if (track != null)
		{
			builder.AppendFormat("{0} track #{1} command #{2}", track.StartGate.Gate.NameSafe(), track.TrackIndex, track.CommandIndex);
		}
		if (command != null)
		{
			builder.AppendFormat(" {0} {1}", command.GetType().Name, command.AssetGuid);
		}
		builder.Append(")");
		string messageFormat = builder.ToString();
		if ((bool)ContextData<EvaluationFailedHandlingFlag>.Current)
		{
			Logger.Exception(Cutscene, e, messageFormat);
			return;
		}
		if (e is FailedToRunCutsceneCommandException && e.InnerException is FailToEvaluateException ex)
		{
			using (ContextData<EvaluationFailedHandlingFlag>.Request())
			{
				CutsceneElement cutsceneElement;
				switch (EvaluationErrorHandlingPolicyHelper.GetEvaluationErrorHandlingPolicy(this, track, command, out cutsceneElement))
				{
				case EvaluationErrorHandlingPolicy.Ignore:
					ElementsDebugger.ClearException(ex.Element, e);
					return;
				case EvaluationErrorHandlingPolicy.SkipTrack:
					if (track == null)
					{
						break;
					}
					ElementsDebugger.ClearException(ex.Element, e);
					track.ForceStop();
					return;
				case EvaluationErrorHandlingPolicy.SkipGate:
					if (cutsceneElement == CutsceneElement.Cutscene)
					{
						ElementsDebugger.ClearException(ex.Element, e);
						Stop();
						return;
					}
					if (track?.StartGate == null)
					{
						break;
					}
					if (track.StartGate.Gate == Cutscene)
					{
						ElementsDebugger.ClearException(ex.Element, e);
						Stop();
					}
					else
					{
						ElementsDebugger.ClearException(ex.Element, e);
						track.ForceGoToEndGate();
					}
					return;
				default:
					throw new ArgumentOutOfRangeException();
				case EvaluationErrorHandlingPolicy.Default:
					break;
				}
			}
		}
		CutsceneLogSink.Instance.PrepareForLog(this, command);
		Logger.Exception(Cutscene, e, messageFormat);
	}

	private static void RaiseEvent<T>(CutscenePlayerData entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<CutscenePlayerData>
	{
		EventBus.RaiseEvent(entity, action, isCheckRuntime);
	}

	private void TryHidePets()
	{
		TryShowPets();
		foreach (BaseUnitEntity item in Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity u) => u.IsPet))
		{
			EntityFact entityFact = item.AddFact(BlueprintRoot.Instance.CutsceneHiddenFeature);
			m_HiddenPetFacts.Add(entityFact);
		}
	}

	private void TryShowPets()
	{
		foreach (EntityFactRef hiddenPetFact in m_HiddenPetFacts)
		{
			EntityFact entityFact = hiddenPetFact;
			entityFact?.Owner.ToEntity().Facts.Remove(entityFact);
		}
		m_HiddenPetFacts.Clear();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<CutscenePlayerData>.GetHash128(m_QueuedAfter);
		result.Append(ref val2);
		result.Append(ref m_IsQueued);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_Cutscene);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<CutscenePlayerGateData>.GetHash128(m_CutsceneData);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<CutscenePlayerGateData>.GetHash128(m_ExitGate);
		result.Append(ref val5);
		List<CutscenePlayerGateData> activatedGates = m_ActivatedGates;
		if (activatedGates != null)
		{
			for (int i = 0; i < activatedGates.Count; i++)
			{
				Hash128 val6 = ClassHasher<CutscenePlayerGateData>.GetHash128(activatedGates[i]);
				result.Append(ref val6);
			}
		}
		Hash128 val7 = NamedParametersContext.Hasher.GetHash128(Parameters);
		result.Append(ref val7);
		result.Append(ref m_Restart);
		result.Append(ref m_Remove);
		bool val8 = Exclusive;
		result.Append(ref val8);
		result.Append(PlayActionId);
		bool val9 = IsFinished;
		result.Append(ref val9);
		List<EntityFactRef> hiddenPetFacts = m_HiddenPetFacts;
		if (hiddenPetFacts != null)
		{
			for (int j = 0; j < hiddenPetFacts.Count; j++)
			{
				EntityFactRef obj = hiddenPetFacts[j];
				Hash128 val10 = StructHasher<EntityFactRef>.GetHash128(ref obj);
				result.Append(ref val10);
			}
		}
		Hash128 val11 = ClassHasher<StatefulRandom>.GetHash128(m_Random);
		result.Append(ref val11);
		return result;
	}
}
