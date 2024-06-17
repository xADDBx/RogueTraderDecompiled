using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public class DialogController : IControllerTick, IController, IControllerStart, IControllerStop, IAreaHandler, ISubscriber, IDialogControllerStartScheduledDialogImmediately
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Dialog");

	private BlueprintCue m_CurrentCue;

	private FogOfWarRevealerSettings m_RevealedSpeaker;

	private string m_CustomSpeakerName;

	private bool m_CapitalPartyChecksEnabled;

	public Vector3 DialogPosition;

	[NotNull]
	public readonly HashSet<BaseUnitEntity> InvolvedUnits = new HashSet<BaseUnitEntity>();

	private readonly List<BlueprintAnswer> m_Answers = new List<BlueprintAnswer>();

	[CanBeNull]
	private BlueprintCueBase m_ContinueCue;

	[NotNull]
	public readonly HashSet<BlueprintCueBase> LocalShownCues = new HashSet<BlueprintCueBase>();

	[NotNull]
	public readonly HashSet<BlueprintAnswer> LocalSelectedAnswers = new HashSet<BlueprintAnswer>();

	[NotNull]
	public readonly HashSet<BlueprintAnswersList> LocalShownAnswerLists = new HashSet<BlueprintAnswersList>();

	[NotNull]
	public readonly HashSet<BlueprintCheck> LocalPassedChecks = new HashSet<BlueprintCheck>();

	[NotNull]
	public readonly HashSet<BlueprintCheck> LocalFailedChecks = new HashSet<BlueprintCheck>();

	private BlueprintCueBase m_CueToPlay;

	private int m_CuesPlayedThisFrame;

	private int m_PlayingBookPageCount;

	private readonly List<CueShowData> m_BookPageCues = new List<CueShowData>();

	[NotNull]
	private readonly Stack<CueSequence> m_Sequences = new Stack<CueSequence>();

	[NotNull]
	private readonly List<SkillCheckResult> m_SkillChecks = new List<SkillCheckResult>();

	[NotNull]
	public readonly List<SoulMarkShift> SoulMarkShifts = new List<SoulMarkShift>();

	private Alignment m_OldPlayerAlignment;

	private float m_FirstSpeakerReturnOrientation;

	private UnitAnimationActionHandle m_CurrentSpeakerAnimation;

	private EntityRef<BaseUnitEntity> m_CurrentSpeaker;

	public BlueprintDialog Dialog { get; private set; }

	private DialogData DialogData { get; set; }

	private CameraRig CameraRig => CameraRig.Instance;

	public int CurrentCueUpdateTick { get; private set; }

	public BlueprintCue CurrentCue
	{
		get
		{
			return m_CurrentCue;
		}
		private set
		{
			if (m_CurrentCue != null)
			{
				m_CurrentCue.OnStop.Run();
				if (m_CurrentCue != value)
				{
					TurnOffSpeakerHighlight();
				}
				if (m_CurrentCue.Speaker.MoveCamera && Game.Instance.CurrentlyLoadedArea.IsPartyArea)
				{
					CameraRig.ScrollTo(DialogPosition);
				}
			}
			if (m_CurrentSpeakerAnimation != null)
			{
				m_CurrentSpeakerAnimation.Release();
				m_CurrentSpeakerAnimation = null;
			}
			m_CurrentCue = value;
			CurrentCueUpdateTick = Game.Instance.RealTimeController.CurrentNetworkTick;
			if (m_CurrentCue == null)
			{
				return;
			}
			CurrentSpeaker = m_CurrentCue.Speaker.GetSpeaker(FirstSpeaker);
			PlayAnimation(CurrentSpeaker, m_CurrentCue.Animation);
			AddInvolvedUnit(CurrentSpeaker);
			m_RevealedSpeaker = null;
			if (!m_CurrentCue.Speaker.NotRevealInFoW && CurrentSpeaker != null && CurrentSpeaker.IsInGame && !CurrentSpeaker.InStealthFor(Game.Instance.Player.Group))
			{
				UnitEntityView view = CurrentSpeaker.View;
				if ((object)view != null)
				{
					FogOfWarRevealerSettings fogOfWarRevealer = view.FogOfWarRevealer;
					if (!fogOfWarRevealer || !fogOfWarRevealer.enabled)
					{
						m_RevealedSpeaker = view.gameObject.AddComponent<FogOfWarRevealerSettings>();
						m_RevealedSpeaker.Enable();
						m_RevealedSpeaker.DefaultRadius = false;
						m_RevealedSpeaker.Radius = 1f;
						FogOfWarControllerData.AddRevealer(m_RevealedSpeaker.transform);
					}
				}
			}
			if (m_CurrentCue.TurnSpeaker)
			{
				TurnUnit(CurrentSpeaker, m_CurrentCue.Listener);
			}
			TryMoveCameraToCurrentSpeaker();
			m_CurrentCue.OnShow.Run();
			m_CurrentCue.ReceiveRewards();
		}
	}

	[CanBeNull]
	private BaseUnitEntity Initiator { get; set; }

	[CanBeNull]
	public BaseUnitEntity FirstSpeaker { get; private set; }

	[CanBeNull]
	public BaseUnitEntity CurrentSpeaker
	{
		get
		{
			return m_CurrentSpeaker;
		}
		private set
		{
			m_CurrentSpeaker = value;
		}
	}

	[CanBeNull]
	private MapObjectView MapObject { get; set; }

	[CanBeNull]
	public BaseUnitEntity ActingUnit { get; private set; }

	public string CurrentSpeakerName
	{
		get
		{
			if (CurrentCue?.Speaker.SpeakerPortrait != null)
			{
				return CurrentCue.Speaker.SpeakerPortrait.CharacterName;
			}
			if (CurrentSpeaker.FromBaseUnitEntity() != null)
			{
				return CurrentSpeaker.CharacterName;
			}
			if (!string.IsNullOrEmpty(m_CustomSpeakerName) && m_CustomSpeakerName != "<null>")
			{
				return m_CustomSpeakerName;
			}
			return string.Empty;
		}
	}

	public BlueprintUnit CurrentSpeakerBlueprint
	{
		get
		{
			object obj = CurrentCue?.Speaker.SpeakerPortrait;
			if (obj == null)
			{
				BaseUnitEntity currentSpeaker = CurrentSpeaker;
				if (currentSpeaker == null)
				{
					return null;
				}
				obj = currentSpeaker.Blueprint;
			}
			return (BlueprintUnit)obj;
		}
	}

	public IEnumerable<BlueprintAnswer> Answers => m_Answers;

	public bool CuePlayScheduled { get; private set; }

	public bool DialogStopScheduled { get; private set; }

	private bool PlayingBookPage => m_PlayingBookPageCount > 0;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Dialog == null)
		{
			StopDialog();
		}
		else if (CuePlayScheduled)
		{
			m_CuesPlayedThisFrame = 0;
			CuePlayScheduled = false;
			PlayCue(m_CueToPlay);
		}
		DialogStopScheduled = false;
	}

	public static DialogData SetupDialogWithUnit([NotNull] BlueprintDialog dialog, [NotNull] BaseUnitEntity unit, [CanBeNull] BaseUnitEntity initiator = null)
	{
		return new DialogData
		{
			Dialog = dialog,
			Initiator = initiator.FromBaseUnitEntity(),
			Unit = unit.FromBaseUnitEntity(),
			MapObject = null,
			CustomSpeakerName = null
		};
	}

	public static DialogData SetupDialogWithMapObject([NotNull] BlueprintDialog dialog, [NotNull] MapObjectView mapObject, [CanBeNull] LocalizedString speakerName, [CanBeNull] BaseUnitEntity initiator = null)
	{
		return new DialogData
		{
			Dialog = dialog,
			Initiator = initiator.FromBaseUnitEntity(),
			Unit = UnitReference.NullUnitReference,
			MapObject = mapObject.Data,
			CustomSpeakerName = speakerName
		};
	}

	public static DialogData SetupDialogWithoutTarget([NotNull] BlueprintDialog dialog, [CanBeNull] LocalizedString speakerName, [CanBeNull] BaseUnitEntity initiator = null)
	{
		return new DialogData
		{
			Dialog = dialog,
			Initiator = initiator.FromBaseUnitEntity(),
			Unit = UnitReference.NullUnitReference,
			MapObject = null,
			CustomSpeakerName = speakerName
		};
	}

	public void StartDialog(DialogData data)
	{
		Logger.Log(data.Dialog, "Requested dialog start {0}", data.Dialog);
		Game.Instance.GameCommandQueue.ScheduleDialogStart(data);
	}

	void IDialogControllerStartScheduledDialogImmediately.StartScheduledDialogImmediately(DialogData dialog)
	{
		StartScheduledDialogImmediately(dialog);
	}

	private void StartScheduledDialogImmediately([NotNull] DialogData scheduled)
	{
		if (scheduled == null)
		{
			throw new ArgumentException("scheduled is null", "scheduled");
		}
		if (CuePlayScheduled)
		{
			m_CuesPlayedThisFrame = 0;
			CuePlayScheduled = false;
			PlayCue(m_CueToPlay);
		}
		BlueprintDialog dialog = scheduled.Dialog;
		BaseUnitEntity initiator = scheduled.Initiator.ToBaseUnitEntity();
		BaseUnitEntity baseUnitEntity = scheduled.Unit.ToBaseUnitEntity();
		BaseUnitEntity currentSpeaker = scheduled.Unit.ToBaseUnitEntity();
		MapObjectView mapObjectView = scheduled.MapObject.Entity?.View;
		string customSpeakerName = scheduled.CustomSpeakerName;
		bool flag = true;
		if (Game.Instance.Player.GameOverReason.HasValue)
		{
			flag = false;
			Logger.Error("Trying to start dialog when the game is over");
		}
		if (Dialog != null)
		{
			flag = false;
			Logger.Error("Trying to start dialog twice. Current {0}, New {1}", Dialog, dialog);
		}
		DialogDebug.Init(dialog);
		if (!dialog.Conditions.Check(Dialog))
		{
			flag = false;
			DialogDebug.Add(dialog, "start conditions failed", Color.red);
		}
		BlueprintCueBase blueprintCueBase = dialog.FirstCue.Select();
		if (blueprintCueBase == null)
		{
			flag = false;
			DialogDebug.Add(Dialog, "could not select first cue", Color.red);
		}
		if (baseUnitEntity != null)
		{
			CutscenePlayerData controllingPlayer = CutsceneControlledUnit.GetControllingPlayer(baseUnitEntity);
			if (controllingPlayer != null && controllingPlayer.Cutscene.ForbidDialogs)
			{
				flag = false;
				DialogDebug.Add(dialog, $"first speaker {baseUnitEntity.Blueprint} is busy in cutscene {controllingPlayer.Cutscene} ({controllingPlayer.Cutscene.AssetGuid})", Color.red);
			}
		}
		using ((baseUnitEntity != null) ? ContextData<ClickedUnitData>.Request().Setup(baseUnitEntity) : null)
		{
			using ((mapObjectView != null) ? ContextData<MechanicEntityData>.Request().Setup(mapObjectView.Data) : null)
			{
				if (!flag)
				{
					dialog.ReplaceActions.Run();
					EventBus.RaiseEvent(delegate(IDialogFinishHandler h)
					{
						h.HandleDialogFinished(dialog, success: false);
					});
					return;
				}
				dialog.StartActions.Run();
			}
		}
		Game.Instance.ProjectileController.Clear();
		Clear();
		Dialog = dialog;
		Initiator = initiator;
		FirstSpeaker = baseUnitEntity;
		CurrentSpeaker = currentSpeaker;
		MapObject = mapObjectView;
		m_CustomSpeakerName = customSpeakerName;
		DialogData = scheduled;
		Logger.Log(dialog, "Trying to start dialog {0}", dialog);
		FillStartPosition();
		AddInvolvedUnit(scheduled.Unit.ToBaseUnitEntity());
		AddInvolvedUnit(scheduled.Initiator.ToBaseUnitEntity());
		if (FirstSpeaker != null && Initiator != null)
		{
			m_FirstSpeakerReturnOrientation = FirstSpeaker.DesiredOrientation;
			if (Dialog.TurnFirstSpeaker)
			{
				TurnUnit(FirstSpeaker, null);
			}
		}
		m_OldPlayerAlignment = Game.Instance.Player.Alignment;
		TryMoveCameraToDialogPosition();
		if (Game.Instance.CurrentMode != GameModeType.Dialog)
		{
			Game.Instance.StartMode(GameModeType.Dialog);
		}
		EventBus.RaiseEvent(delegate(IDialogInteractionHandler h)
		{
			h.StartDialogInteraction(dialog);
		});
		ScheduleCue(blueprintCueBase);
		DialogDebug.Add(Dialog, "Started dialog", Color.green);
		Game.Instance.Player.Dialog.ShownDialogs.Add(Dialog);
	}

	public void TryMoveCameraToCurrentSpeaker()
	{
		if (!(CameraRig == null) && m_CurrentCue != null && m_CurrentCue?.Speaker != null && CurrentSpeaker != null && Game.Instance?.CurrentlyLoadedArea != null && (bool)CameraRig && m_CurrentCue != null && m_CurrentCue.Speaker.MoveCamera && CurrentSpeaker != null && Game.Instance.CurrentlyLoadedArea.IsPartyArea)
		{
			float valueOrDefault = (BlueprintRoot.Instance?.Dialog?.GetCameraOffsetBySize(CurrentSpeaker.Size)).GetValueOrDefault();
			Vector3 position = CurrentSpeaker.Position + Vector3.up * valueOrDefault;
			CameraRig.ScrollTo(position);
		}
	}

	private void TryMoveCameraToDialogPosition()
	{
		if (!(CameraRig == null) && Game.Instance.CurrentlyLoadedArea != null && (bool)CameraRig && Game.Instance.CurrentlyLoadedArea.IsPartyArea)
		{
			Vector2 viewportOffset = new Vector2(0f, UIConfig.Instance.DialogCameraYCorrection);
			CameraRig.SetViewportOffset(viewportOffset);
			CameraRig.ScrollTo(DialogPosition);
		}
	}

	private void FillStartPosition()
	{
		bool flag = false;
		if (Dialog.StartPosition != null)
		{
			try
			{
				DialogPosition = Dialog.StartPosition.GetValue();
				flag = true;
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		if (!flag)
		{
			if (FirstSpeaker != null)
			{
				DialogPosition = FirstSpeaker.Position;
			}
			else if (MapObject != null)
			{
				DialogPosition = MapObject.ViewTransform.position;
			}
			else if (Game.Instance.IsModeActive(GameModeType.GlobalMap))
			{
				DialogPosition = CameraRig.Instance.transform.position;
			}
			else
			{
				DialogPosition = Game.Instance.Player.MainCharacter.Entity.Position;
			}
		}
	}

	private void ScheduleCue(BlueprintCueBase cue)
	{
		m_CueToPlay = cue;
		CuePlayScheduled = true;
	}

	private void AddInvolvedUnit([CanBeNull] BaseUnitEntity unit)
	{
		if (unit != null)
		{
			if (CutsceneControlledUnit.GetControllingPlayer(unit) == null)
			{
				unit.Commands.InterruptAllInterruptible();
			}
			unit.Stealth.ForceExitStealth();
			InvolvedUnits.Add(unit);
		}
	}

	private void Clear()
	{
		DialogData?.Dispose();
		Dialog = null;
		DialogData = null;
		CurrentCue = null;
		ActingUnit = null;
		m_CapitalPartyChecksEnabled = false;
		m_CueToPlay = null;
		CuePlayScheduled = false;
		m_Answers.Clear();
		m_ContinueCue = null;
		m_Sequences.Clear();
		m_BookPageCues.Clear();
		m_SkillChecks.Clear();
		SoulMarkShifts.Clear();
		InvolvedUnits.Clear();
		m_PlayingBookPageCount = 0;
		LocalShownCues.Clear();
		LocalSelectedAnswers.Clear();
		LocalShownAnswerLists.Clear();
		LocalPassedChecks.Clear();
		LocalFailedChecks.Clear();
		DialogPosition = Vector3.zero;
		TurnOffSpeakerHighlight();
	}

	private void TurnOffSpeakerHighlight()
	{
		if (m_RevealedSpeaker != null)
		{
			FogOfWarControllerData.RemoveRevealer(m_RevealedSpeaker.transform);
			UnityEngine.Object.Destroy(m_RevealedSpeaker);
			m_RevealedSpeaker = null;
		}
	}

	public void StopDialog()
	{
		if (DialogStopScheduled)
		{
			return;
		}
		DialogStopScheduled = true;
		BlueprintDialog dialog = Dialog;
		CameraRig instance = CameraRig.Instance;
		if (instance != null)
		{
			instance.SetViewportOffset(Vector2.zero);
		}
		if (!Game.Instance.GameCommandQueue.ContainsCommand((StartScheduledDialogCommand _) => true))
		{
			Game.Instance.StopMode(GameModeType.Dialog);
		}
		try
		{
			if (dialog != null)
			{
				EventBus.RaiseEvent(delegate(IDialogInteractionHandler h)
				{
					h.StopDialogInteraction(dialog);
				});
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(Dialog, ex);
		}
		if (FirstSpeaker != null && Initiator != null)
		{
			FirstSpeaker.DesiredOrientation = m_FirstSpeakerReturnOrientation;
		}
		dialog?.FinishActions.Run();
		Clear();
	}

	public void SelectAnswer(string answerGuid)
	{
		int i = 0;
		for (int count = m_Answers.Count; i < count; i++)
		{
			BlueprintAnswer blueprintAnswer = m_Answers[i];
			if (blueprintAnswer.AssetGuid == answerGuid)
			{
				SelectAnswer(blueprintAnswer);
				break;
			}
		}
	}

	public void SelectAnswer(BlueprintAnswer answer, BaseUnitEntity manualUnitSelection = null)
	{
		if (CurrentCue == null)
		{
			return;
		}
		if (!answer.IsSystem() && !m_Answers.Contains(answer))
		{
			PFLog.Default.Error("Trying to select invalid dialog answer {0}", answer);
			return;
		}
		if (manualUnitSelection == null && answer.CharacterSelection.SelectionType == CharacterSelection.Type.Manual)
		{
			PFLog.Default.Error("A unit must be specified for selected answer. Answer: {0}", answer);
			return;
		}
		DialogDebug.Init(Dialog);
		AddHistoryEntry(CurrentCue, CurrentSpeakerName);
		AddHistoryEntry(answer, CurrentCue.Listener?.CharacterName);
		Game.Instance.Player.Dialog.SelectedAnswers.Add(answer);
		LocalSelectedAnswers.Add(answer);
		Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> bookEventLog = Game.Instance.Player.Dialog.BookEventLog;
		if (bookEventLog.ContainsKey(Dialog))
		{
			bookEventLog[Dialog].Add(answer);
		}
		ActingUnit = answer.CharacterSelection.SelectUnit(answer, manualUnitSelection, forceManual: true);
		if (answer.CharacterSelection.SelectionType != CharacterSelection.Type.Keep)
		{
			m_CapitalPartyChecksEnabled = answer.CapitalPartyChecksEnabled;
		}
		try
		{
			answer.ApplyShiftDialog();
		}
		catch (Exception ex)
		{
			PFLog.Default.Error(ex, "Can't do soulmark shift");
		}
		if (NoNextCue(answer))
		{
			ScheduleCue(null);
		}
		answer.OnSelect.Run();
		answer.ApplyRequirements();
		answer.ReceiveRewards();
		BlueprintCueBase cue = SelectNextCue(answer);
		ScheduleCue(cue);
		EventBus.RaiseEvent(delegate(ISelectAnswerHandler h)
		{
			h.HandleSelectAnswer(answer);
		});
	}

	[CanBeNull]
	private BlueprintCueBase SelectNextCue([NotNull] BlueprintAnswer answer)
	{
		if (answer.IsContinue())
		{
			if (m_ContinueCue == null)
			{
				PFLog.Default.Error("Continue answer was selected but continue cue is not specified");
			}
			return m_ContinueCue;
		}
		BlueprintCueBase blueprintCueBase = (answer.IsExit() ? null : answer.NextCue.Select());
		while (blueprintCueBase == null && m_Sequences.Any())
		{
			CueSequence cueSequence = m_Sequences.Peek();
			blueprintCueBase = cueSequence.PollNextCue();
			if (blueprintCueBase == null)
			{
				m_Sequences.Pop();
				blueprintCueBase = cueSequence.Blueprint.Exit?.Continue.Select();
			}
		}
		return blueprintCueBase;
	}

	private bool NoNextCue([NotNull] BlueprintAnswer answer)
	{
		if (answer.IsContinue())
		{
			return m_ContinueCue == null;
		}
		if (answer.IsExit() || answer.NextCue.Cues.EmptyIfNull().Empty())
		{
			return m_Sequences.Empty();
		}
		return false;
	}

	private void AddHistoryEntry([NotNull] BlueprintAnswer answer, [CanBeNull] string listenerName = null)
	{
		if (answer.AddToHistory)
		{
			DialogType type = Dialog.Type;
			AnswerShowData answerShowData;
			if (type == DialogType.Common || type == DialogType.StarSystemEvent)
			{
				BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
				string characterName = listenerName ?? playerCharacter.CharacterName;
				answerShowData = new AnswerShowData(answer, Dialog.Type, characterName, playerCharacter.Blueprint);
			}
			else
			{
				answerShowData = new AnswerShowData(answer, Dialog.Type);
			}
			EventBus.RaiseEvent(delegate(IDialogHistoryHandler h)
			{
				h.HandleOnDialogHistory(answerShowData);
			});
		}
	}

	private void AddHistoryEntry([NotNull] BlueprintCue cue, string speakerName)
	{
		CueShowData cueShowData = ((!string.IsNullOrEmpty(speakerName)) ? new CueShowData(cue, CurrentSpeakerName, CurrentSpeakerBlueprint) : new CueShowData(cue));
		EventBus.RaiseEvent(delegate(IDialogHistoryHandler h)
		{
			h.HandleOnDialogHistory(cueShowData);
		});
	}

	private void PlayCue(BlueprintCueBase cue)
	{
		if (m_CuesPlayedThisFrame++ > 1000)
		{
			throw new InvalidOperationException($"Stack overflow while playing dialog cues. Dialog: {Dialog}, One of cues: {cue}");
		}
		SoundState.Instance.StopDialog();
		if (cue == null)
		{
			StopDialog();
			return;
		}
		LocalShownCues.Add(cue);
		Game.Instance.Player.Dialog.ShownCues.Add(cue);
		m_Answers.Clear();
		m_ContinueCue = null;
		DialogDebug.Add(cue, "played", Color.green);
		if (cue is BlueprintCue cue2)
		{
			PlayBasicCue(cue2);
		}
		else if (cue is BlueprintCheck check)
		{
			PlayCheck(check);
		}
		else if (cue is BlueprintCueSequence sequence)
		{
			PlaySequence(sequence);
		}
		else if (cue is BlueprintBookPage page)
		{
			PlayBookPage(page);
		}
	}

	private void AddAnswers([NotNull] IEnumerable<BlueprintAnswerBase> answers, [CanBeNull] BlueprintCueBase continueCue)
	{
		if (continueCue == null)
		{
			foreach (BlueprintAnswerBase answer2 in answers)
			{
				if (answer2 is BlueprintAnswersList blueprintAnswersList && blueprintAnswersList.CanSelect())
				{
					Game.Instance.Player.Dialog.ShownAnswerLists.Add(blueprintAnswersList);
					LocalShownAnswerLists.Add(blueprintAnswersList);
					AddAnswers(blueprintAnswersList.Answers.Dereference(), null);
					EventBus.RaiseEvent(delegate(IDialogAnswersShownHandler i)
					{
						i.HandleAnswersShown();
					});
					return;
				}
			}
		}
		m_Answers.Clear();
		m_ContinueCue = null;
		if ((bool)continueCue)
		{
			m_Answers.Add(Dialog.GetContinueAnswer());
			m_ContinueCue = continueCue;
		}
		else
		{
			foreach (BlueprintAnswerBase answer3 in answers)
			{
				BlueprintAnswer answer = answer3 as BlueprintAnswer;
				if (answer != null && answer.CanShow())
				{
					m_Answers.Add(answer);
					EventBus.RaiseEvent(delegate(IDialogAnswersAddedToPoolHandler h)
					{
						h.HandleDialogAnswersAddedToPool(answer);
					});
				}
			}
		}
		if (!Answers.Empty())
		{
			return;
		}
		if (m_Sequences.Count > 0)
		{
			CueSequence cueSequence = m_Sequences.Peek();
			BlueprintCueBase blueprintCueBase = cueSequence.PollNextCue();
			if (blueprintCueBase != null)
			{
				m_Answers.Add(Dialog.GetContinueAnswer());
				m_ContinueCue = blueprintCueBase;
				return;
			}
			m_Sequences.Pop();
			BlueprintSequenceExit exit = cueSequence.Blueprint.Exit;
			if ((bool)exit)
			{
				AddAnswers(exit.Answers.Dereference(), exit.Continue.Select());
			}
			else
			{
				m_Answers.Add(Dialog.GetExitAnswer());
			}
		}
		else
		{
			m_Answers.Add(Dialog.GetExitAnswer());
		}
	}

	private void PlayBasicCue(BlueprintCue cue)
	{
		CurrentCue = cue;
		cue.ApplyShiftDialog();
		BlueprintCueBase blueprintCueBase = cue.Continue.Select();
		CueShowData cueShowData = new CueShowData(cue, m_SkillChecks, SoulMarkShifts);
		Alignment alignment = Game.Instance.Player.Alignment;
		if (m_OldPlayerAlignment != alignment)
		{
			cueShowData.NewAlignment = alignment;
			m_OldPlayerAlignment = alignment;
		}
		m_SkillChecks.Clear();
		SoulMarkShifts.Clear();
		if (!PlayingBookPage)
		{
			AddAnswers(cue.Answers.Dereference(), blueprintCueBase);
			EventBus.RaiseEvent(delegate(IDialogCueHandler h)
			{
				h.HandleOnCueShow(cueShowData);
			});
		}
		else
		{
			m_BookPageCues.Add(cueShowData);
			Game.Instance.Player.Dialog.BookEventLog[Dialog].Add(cue);
		}
		if (PlayingBookPage && (bool)blueprintCueBase)
		{
			PlayCue(blueprintCueBase);
		}
	}

	private SkillCheckResult ExecuteCheck(BlueprintCheck check)
	{
		BaseUnitEntity baseUnitEntity = check.GetTargetUnit() ?? ActingUnit;
		try
		{
			if (baseUnitEntity != null)
			{
				return new SkillCheckResult(GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(baseUnitEntity, check.Type, check.GetDC()), null, allowPartyCheckInCamp: false), baseUnitEntity);
			}
			RulePerformPartySkillCheck rulePerformPartySkillCheck = new RulePerformPartySkillCheck(check.Type, check.GetDC(), m_CapitalPartyChecksEnabled);
			Game.Instance.Rulebook.TriggerEvent(rulePerformPartySkillCheck);
			return new SkillCheckResult(rulePerformPartySkillCheck);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed to perform skill check {0}", check);
			return new SkillCheckResult(baseUnitEntity ?? Game.Instance.Player.MainCharacterEntity, check.Type, check.GetDC());
		}
	}

	private void PlayCheck(BlueprintCheck check)
	{
		SkillCheckResult skillCheckResult = ExecuteCheck(check);
		if (skillCheckResult.Passed)
		{
			LocalPassedChecks.Add(check);
			LocalFailedChecks.Remove(check);
			GameHelper.GainExperienceForSkillCheck(ExperienceHelper.GetXp(EncounterType.SkillCheck, ExperienceHelper.GetCheckExp(skillCheckResult.DC, Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0)), skillCheckResult.ActingUnit);
			check.OnCheckSuccessActions?.Run();
		}
		else if (!LocalPassedChecks.Contains(check))
		{
			LocalFailedChecks.Add(check);
			check.OnCheckFailActions?.Run();
		}
		if (!check.Hidden || skillCheckResult.Passed)
		{
			m_SkillChecks.Add(skillCheckResult);
		}
		PlayCue(skillCheckResult.Passed ? check.Success : check.Fail);
	}

	private void PlaySequence(BlueprintCueSequence sequence)
	{
		CueSequence cueSequence = new CueSequence(sequence);
		m_Sequences.Push(cueSequence);
		BlueprintCueBase blueprintCueBase = cueSequence.PollNextCue();
		if (blueprintCueBase == null)
		{
			PFLog.Default.Error("Could not select first cue in cue sequence ({0}).", sequence);
			StopDialog();
		}
		else
		{
			PlayCue(blueprintCueBase);
		}
	}

	private void PlayBookPage(BlueprintBookPage page)
	{
		try
		{
			m_PlayingBookPageCount++;
			Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> bookEventLog = Game.Instance.Player.Dialog.BookEventLog;
			if (!bookEventLog.ContainsKey(Dialog))
			{
				bookEventLog.Add(Dialog, new List<BlueprintScriptableObject>());
			}
			bookEventLog[Dialog].Add(page);
			page.OnShow.Run();
			IEnumerable<BlueprintCueBase> enumerable = from cue in page.Cues.Dereference()
				where cue.CanShow()
				select cue;
			bool flag = false;
			foreach (BlueprintCueBase item in enumerable)
			{
				PlayCue(item);
				flag = true;
			}
			if (!flag)
			{
				LogChannel.Default.ErrorWithReport("Could not select any cue in book page ({0}).", page);
				StopDialog();
			}
			else
			{
				AddAnswers(page.Answers.Dereference(), null);
				EventBus.RaiseEvent(delegate(IBookPageHandler h)
				{
					h.HandleOnBookPageShow(page, m_BookPageCues, m_Answers);
				});
			}
		}
		finally
		{
			m_BookPageCues.Clear();
			m_PlayingBookPageCount--;
		}
	}

	private void TurnUnit([CanBeNull] BaseUnitEntity unit, [CanBeNull] BlueprintUnit listener)
	{
		if (unit == null || !unit.IsInGame)
		{
			return;
		}
		float listenerRange = BlueprintRoot.Instance.Dialog.ListenerRange;
		BaseUnitEntity baseUnitEntity = null;
		if (listener != null)
		{
			baseUnitEntity = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity u) => u.Blueprint == listener).Nearest(unit.Position);
			if (baseUnitEntity != null && unit.DistanceTo(baseUnitEntity) > listenerRange)
			{
				baseUnitEntity = null;
			}
		}
		if (baseUnitEntity != null)
		{
			unit.LookAt(baseUnitEntity.Position);
			baseUnitEntity.Stealth.ForceExitStealth();
			return;
		}
		if (unit.IsDirectlyControllable)
		{
			Vector3 point = FirstSpeaker?.Position ?? DialogPosition;
			unit.LookAt(point);
			return;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		BaseUnitEntity baseUnitEntity2 = ((unit.DistanceTo(mainCharacterEntity) <= listenerRange || Initiator == null) ? mainCharacterEntity : Initiator);
		unit.LookAt(baseUnitEntity2.Position);
		baseUnitEntity2.Stealth.ForceExitStealth();
	}

	private void PlayAnimation([CanBeNull] BaseUnitEntity unit, DialogAnimation animation)
	{
		if (!(unit?.View == null) && !(unit.View.AnimationManager == null) && animation != 0)
		{
			m_CurrentSpeakerAnimation = unit.View.AnimationManager.CreateHandle(UnitAnimationType.Dialog);
			if (m_CurrentSpeakerAnimation == null)
			{
				PFLog.Default.ErrorWithReport($"DialogCueAnimation is missing (dialog: {Dialog.name}, unit: {unit})");
				return;
			}
			m_CurrentSpeakerAnimation.Variant = (int)animation;
			unit.View.AnimationManager.Execute(m_CurrentSpeakerAnimation);
		}
	}

	public void OnStart()
	{
		UnitCombatLeaveController.TickGroups(ignoreTimer: true);
		if (Dialog.TurnPlayer)
		{
			TurnUnit(Game.Instance.Player.MainCharacterEntity, null);
			TurnUnit(Initiator, null);
		}
		if (FirstSpeaker != null)
		{
			AddInvolvedUnit(FirstSpeaker);
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (CutsceneControlledUnit.GetControllingPlayer(allUnit) == null)
			{
				allUnit.Commands.InterruptAllInterruptible();
			}
		}
		NetService.Instance.CancelCurrentCommands();
		EventBus.RaiseEvent(delegate(IDialogStartHandler h)
		{
			h.HandleDialogStarted(Dialog);
		});
	}

	public void OnStop()
	{
		List<BaseUnitEntity> list = InvolvedUnits.ToList();
		foreach (BaseUnitEntity involvedUnit in InvolvedUnits)
		{
			StopDialogAnimations(involvedUnit);
		}
		DialogData?.Dispose();
		SoundState.Instance.StopDialog();
		EventBus.RaiseEvent(delegate(IDialogFinishHandler h)
		{
			h.HandleDialogFinished(Dialog, success: true);
		});
		m_CurrentCue = null;
		Clear();
		foreach (BaseUnitEntity item in list)
		{
			CutsceneControlledUnit.UpdateActiveCutscene(item);
		}
	}

	private static void StopDialogAnimations(BaseUnitEntity unit)
	{
		IReadOnlyList<AnimationActionHandle> readOnlyList = ((unit.View != null && unit.View.AnimationManager != null) ? unit.View.AnimationManager.ActiveActions : null);
		if (readOnlyList == null)
		{
			return;
		}
		foreach (UnitAnimationActionHandle item in readOnlyList)
		{
			if (item.Action.Type == UnitAnimationType.Dialog)
			{
				item.Release();
			}
		}
	}

	public void Dispose()
	{
		m_CurrentCue = null;
		Clear();
	}

	public void OnAreaBeginUnloading()
	{
		if (Dialog == null)
		{
			Initiator = null;
			FirstSpeaker = null;
			CurrentSpeaker = null;
			ActingUnit = null;
			m_CapitalPartyChecksEnabled = false;
		}
	}

	public void OnAreaDidLoad()
	{
	}

	[Cheat(Name = "dialog_force", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StartDialogCheat(BlueprintDialog dialog)
	{
		DialogData data = SetupDialogWithoutTarget(dialog, null);
		Game.Instance.DialogController.StartDialog(data);
	}
}
