using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;

public class DialogVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IDialogCueHandler, ISubscriber, IDialogHistoryHandler, IGameModeHandler, INetPingDialogAnswer, IFullScreenUIHandler
{
	public readonly ReactiveProperty<Sprite> SpeakerPortrait = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<Sprite> AnswerPortrait = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> SpeakerName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> AnswerName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> EmptySpeaker = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsSpeakerNeedGlow = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsAnswererNeedGlow = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsSpeakerNeedEqualizer = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsAnswererNeedEqualizer = new ReactiveProperty<bool>();

	public readonly ReactiveCollection<IDialogShowData> History = new ReactiveCollection<IDialogShowData>();

	public readonly ReactiveProperty<List<AnswerVM>> Answers = new ReactiveProperty<List<AnswerVM>>(null);

	public readonly ReactiveProperty<AnswerVM> SystemAnswer = new ReactiveProperty<AnswerVM>(null);

	public readonly ReactiveProperty<CueVM> Cue = new ReactiveProperty<CueVM>(null);

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveCommand OnCueUpdate = new ReactiveCommand();

	public readonly DialogNotificationsVM DialogNotifications;

	public readonly DialogVotesBlockVM DialogVotesBlockVM;

	public readonly ReactiveCommand CheckVotesActive = new ReactiveCommand();

	public readonly ReactiveProperty<CharGenPortraitVM> FullPortraitVM = new ReactiveProperty<CharGenPortraitVM>();

	public readonly BoolReactiveProperty IsSpeakerFullPortraitShowed = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsAnswererFullPortraitShowed = new BoolReactiveProperty();

	public readonly BoolReactiveProperty SpeakerHasPortrait = new BoolReactiveProperty();

	public readonly BoolReactiveProperty AnswererHasPortrait = new BoolReactiveProperty();

	private PortraitData m_SpeakerPortraitData;

	private PortraitData m_AnswererPortraitData;

	public event Action UpdateView;

	public DialogVM()
	{
		AddDisposable(DialogNotifications = new DialogNotificationsVM());
		AddDisposable(DialogVotesBlockVM = new DialogVotesBlockVM());
		AddDisposable(OnCueUpdate.Subscribe(delegate
		{
			UpdatePortrait();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		IsSpeakerFullPortraitShowed.Value = false;
		IsAnswererFullPortraitShowed.Value = false;
		m_SpeakerPortraitData = null;
		m_AnswererPortraitData = null;
		SpeakerHasPortrait.Value = false;
		AnswererHasPortrait.Value = false;
		ClearPortrait();
		DisposeCue();
		DisposeAnswers();
	}

	private void DisposeCue()
	{
		Cue.Value?.Dispose();
		Cue.Value = null;
	}

	private void DisposeAnswers()
	{
		Answers.Value?.ForEach(delegate(AnswerVM v)
		{
			v.Dispose();
		});
		Answers.Value = null;
		SystemAnswer.Value?.Dispose();
		SystemAnswer.Value = null;
	}

	public void HandleOnCueShow(CueShowData data)
	{
		DisposeCue();
		DisposeAnswers();
		ResetPortraitsGlow();
		BlueprintCue cue = data.Cue;
		if (!cue)
		{
			return;
		}
		DialogController dialogController = Game.Instance.DialogController;
		BaseUnitEntity currentSpeaker = dialogController.CurrentSpeaker;
		BlueprintUnit speakerPortrait = cue.Speaker.SpeakerPortrait;
		Sprite value = null;
		if (speakerPortrait != null)
		{
			value = (speakerPortrait.PortraitSafe.InitiativePortrait ? null : speakerPortrait.PortraitSafe.HalfLengthPortrait);
		}
		else if (currentSpeaker != null)
		{
			value = (currentSpeaker.Portrait.InitiativePortrait ? null : currentSpeaker.Portrait.HalfLengthPortrait);
		}
		SpeakerPortrait.Value = value;
		SpeakerName.Value = ((speakerPortrait != null) ? speakerPortrait.CharacterName : currentSpeaker?.CharacterName);
		EmptySpeaker.Value = SpeakerPortrait.Value == null && SpeakerName.Value == null;
		IsSpeakerNeedGlow.Value = currentSpeaker == cue.Speaker?.GetEntity() && SpeakerPortrait.Value != null;
		IsAnswererNeedGlow.Value = currentSpeaker?.CharacterName == cue.Listener?.CharacterName && AnswerPortrait.Value != null;
		bool flag = cue.Speaker?.GetEntity() == null || currentSpeaker == cue.Speaker?.GetEntity();
		IsSpeakerNeedEqualizer.Value = flag && SpeakerPortrait.Value == null;
		bool flag2 = currentSpeaker?.CharacterName == cue.Listener?.CharacterName;
		IsAnswererNeedEqualizer.Value = flag2 && AnswerPortrait.Value == null;
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		BaseUnitEntity listenerEntity = UIUtilityGetDialogListenerEntity.GetListenerEntity(cue.Listener);
		Sprite value2 = mainCharacterEntity.Portrait.HalfLengthPortrait;
		if (cue.Listener != null && cue.Listener.name != "Player Character")
		{
			value2 = ((listenerEntity == null) ? (cue.Listener.PortraitSafe.InitiativePortrait ? null : cue.Listener.PortraitSafe.HalfLengthPortrait) : (listenerEntity.Portrait.InitiativePortrait ? null : listenerEntity?.Portrait?.HalfLengthPortrait));
		}
		AnswerPortrait.Value = value2;
		AnswerName.Value = ((cue.Listener == null || !(cue.Listener.name != "Player Character")) ? mainCharacterEntity.CharacterName : ((listenerEntity == null) ? cue.Listener.CharacterName : listenerEntity?.CharacterName));
		BlueprintAnswer blueprintAnswer = dialogController.Answers.FirstOrDefault();
		if (blueprintAnswer != null && blueprintAnswer.IsSystem())
		{
			Answers.Value = null;
			SystemAnswer.Value = new AnswerVM(blueprintAnswer, Game.Instance.DialogController, 1);
		}
		else
		{
			int index = 1;
			Answers.Value = (from a in dialogController.Answers
				where !a.IsSystem()
				select a into answer
				select new AnswerVM(answer, Game.Instance.DialogController, index++)).ToList();
			SystemAnswer.Value = null;
		}
		GameObject target = ((currentSpeaker != null && currentSpeaker.View != null) ? currentSpeaker.View.gameObject : null);
		Cue.Value = new CueVM(cue, data.SkillChecks, data.SoulMarkShifts);
		VoiceOverPlayer.PlayVoiceOver(cue.Text, target);
		PFLog.UI.Log($"[{Time.frameCount}] HandleOnCueShow OnCueUpdate");
		this.UpdateView?.Invoke();
		OnCueUpdate.Execute();
	}

	public void HandleOnDialogHistory(IDialogShowData showData)
	{
		History.Add(showData);
	}

	private void ResetPortraitsGlow()
	{
		IsSpeakerNeedGlow.Value = false;
		IsAnswererNeedGlow.Value = false;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.EscapeMenu)
		{
			bool num = Game.Instance.CurrentMode == GameModeType.Cutscene || Game.Instance.CurrentMode == GameModeType.CutsceneGlobalMap;
			CutscenePlayerData cutscenePlayerData = Game.Instance.State?.Cutscenes?.LastOrDefault();
			bool flag = false;
			if (num && cutscenePlayerData != null)
			{
				flag = !cutscenePlayerData.IsFinished && !cutscenePlayerData.Paused && cutscenePlayerData.Cutscene.LockControl;
			}
			IsVisible.Value = !state && Game.Instance.IsModeActive(GameModeType.Dialog) && !flag;
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.CutsceneGlobalMap || Game.Instance.IsPaused)
		{
			IsVisible.Value = false;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			IsVisible.Value = true;
			Game.Instance.DialogController?.TryMoveCameraToCurrentSpeaker();
		}
	}

	public void HandleShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void HandleDialogAnswerHover(string answer, bool hover)
	{
	}

	public void HandleDialogAnswerVote(NetPlayer player, string answer)
	{
		CheckVotesActive.Execute();
	}

	public void ShowHideBigScreenshotSpeaker(bool state)
	{
		IsSpeakerFullPortraitShowed.Value = state && m_SpeakerPortraitData != null && m_SpeakerPortraitData.FullLengthPortrait != null;
		IsAnswererFullPortraitShowed.Value = false;
		if (!state || m_SpeakerPortraitData == null || m_SpeakerPortraitData.FullLengthPortrait == null)
		{
			ClearPortrait();
		}
		else
		{
			FullPortraitVM.Value = new CharGenPortraitVM(m_SpeakerPortraitData);
		}
	}

	public void ShowHideBigScreenshotAnswerer(bool state)
	{
		IsAnswererFullPortraitShowed.Value = state && m_AnswererPortraitData != null && m_AnswererPortraitData.FullLengthPortrait != null;
		IsSpeakerFullPortraitShowed.Value = false;
		if (!state || m_AnswererPortraitData == null || m_AnswererPortraitData.FullLengthPortrait == null)
		{
			ClearPortrait();
		}
		else
		{
			FullPortraitVM.Value = new CharGenPortraitVM(m_AnswererPortraitData);
		}
	}

	private void UpdatePortrait()
	{
		CheckSpeakerPortrait();
		CheckAnswererPortrait();
		if (IsSpeakerFullPortraitShowed.Value || IsAnswererFullPortraitShowed.Value)
		{
			if (IsSpeakerFullPortraitShowed.Value)
			{
				ShowHideBigScreenshotSpeaker(state: true);
			}
			else if (IsAnswererFullPortraitShowed.Value)
			{
				ShowHideBigScreenshotAnswerer(state: true);
			}
		}
	}

	private void CheckSpeakerPortrait()
	{
		BaseUnitEntity currentSpeaker = Game.Instance.DialogController.CurrentSpeaker;
		BlueprintUnit speakerPortrait = Cue.Value.BlueprintCue.Speaker.SpeakerPortrait;
		m_SpeakerPortraitData = null;
		if (speakerPortrait != null)
		{
			m_SpeakerPortraitData = (speakerPortrait.PortraitSafe.InitiativePortrait ? null : speakerPortrait.PortraitSafe.Data);
		}
		else if (currentSpeaker != null)
		{
			m_SpeakerPortraitData = (currentSpeaker.Portrait.InitiativePortrait ? null : currentSpeaker.Portrait);
		}
		SpeakerHasPortrait.Value = m_SpeakerPortraitData != null && m_SpeakerPortraitData.FullLengthPortrait != null;
		if (m_SpeakerPortraitData == null || m_SpeakerPortraitData.FullLengthPortrait == null)
		{
			ClearPortrait();
		}
	}

	private void CheckAnswererPortrait()
	{
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		m_AnswererPortraitData = mainCharacterEntity.Portrait;
		BaseUnitEntity listenerEntity = UIUtilityGetDialogListenerEntity.GetListenerEntity(Cue?.Value?.BlueprintCue?.Listener);
		if (Cue?.Value?.BlueprintCue?.Listener != null && Cue.Value.BlueprintCue.Listener.name != "Player Character")
		{
			m_AnswererPortraitData = ((listenerEntity == null) ? (Cue.Value.BlueprintCue.Listener.PortraitSafe.InitiativePortrait ? null : Cue.Value.BlueprintCue.Listener.PortraitSafe.Data) : (listenerEntity.Portrait.InitiativePortrait ? null : listenerEntity.Portrait));
		}
		AnswererHasPortrait.Value = m_AnswererPortraitData != null && m_AnswererPortraitData.FullLengthPortrait != null;
		if (m_AnswererPortraitData == null || m_AnswererPortraitData.FullLengthPortrait == null)
		{
			ClearPortrait();
		}
	}

	private void ClearPortrait()
	{
		FullPortraitVM.Value?.Dispose();
		FullPortraitVM.Value = null;
	}
}
