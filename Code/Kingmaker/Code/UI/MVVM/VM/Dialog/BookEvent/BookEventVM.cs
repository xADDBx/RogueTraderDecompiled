using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Sound.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;

public class BookEventVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBookPageHandler, ISubscriber, IBookEventUIHandler, IGameModeHandler, INetPingDialogAnswer
{
	public readonly ReactiveProperty<bool> ChooseCharacterActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<BlueprintBookPage> BlueprintBookPage = new ReactiveProperty<BlueprintBookPage>();

	public readonly ReactiveProperty<Sprite> EventPicture = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<List<AnswerVM>> Answers = new ReactiveProperty<List<AnswerVM>>();

	public readonly ReactiveProperty<Sprite> Mirror = new ReactiveProperty<Sprite>(null);

	public readonly ReactiveCollection<CueVM> Cues = new ReactiveCollection<CueVM>();

	public readonly ReactiveCollection<CueVM> HistoryCues = new ReactiveCollection<CueVM>();

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<string> ChoosedAnswer = new ReactiveProperty<string>();

	public readonly DialogNotificationsVM DialogNotifications;

	public BookEventChooseCharacterVM BookEventChooseCharacter;

	[CanBeNull]
	private VoiceOverStatus m_PlayingVoiceOver;

	[NotNull]
	private readonly Queue<BlueprintCue> m_VoiceOverQueue = new Queue<BlueprintCue>();

	private bool m_IsFirstCueInAllBookEvent;

	public readonly ReactiveCommand CheckVotesActive = new ReactiveCommand();

	private Sprite DefaultSprite => BlueprintRoot.Instance.Dialog.DefaultBookEventPicture;

	public BookEventVM()
	{
		m_IsFirstCueInAllBookEvent = true;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(DialogNotifications = new DialogNotificationsVM());
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdate();
		}));
		AddDisposable(AnswerVM.ChoosedAnswer.Skip(1).Subscribe(delegate
		{
			ChoosedAnswer.Value = AnswerVM.ChoosedAnswer.Value;
		}));
	}

	protected override void DisposeImplementation()
	{
		ClearAnswers();
		ClearCues();
		ClearHistoryCues();
	}

	private void OnUpdate()
	{
		if (m_PlayingVoiceOver == null || m_PlayingVoiceOver.IsEnded)
		{
			m_PlayingVoiceOver = null;
			while (m_PlayingVoiceOver == null && m_VoiceOverQueue.Count > 0)
			{
				m_PlayingVoiceOver = VoiceOverPlayer.PlayVoiceOver(m_VoiceOverQueue.Dequeue().Text);
			}
		}
	}

	protected virtual void SetPage(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		BlueprintBookPage.Value = page;
		SetCues(cues);
		SetVoiceCues(cues);
		SetAnswers(answers);
		SetPicture(page);
		SetMirror(page);
	}

	private void ClearCues()
	{
		Cues.ForEach(delegate(CueVM d)
		{
			d.Dispose();
		});
		Cues.Clear();
		m_PlayingVoiceOver?.Stop();
		m_PlayingVoiceOver = null;
		m_VoiceOverQueue.Clear();
	}

	public void ClearHistoryCues()
	{
		HistoryCues.ForEach(delegate(CueVM d)
		{
			d.Dispose();
		});
		HistoryCues.Clear();
	}

	private void SetVoiceCues(List<CueShowData> cues)
	{
		foreach (CueShowData cue in cues)
		{
			m_VoiceOverQueue.Enqueue(cue.Cue);
		}
	}

	protected virtual void SetCues(List<CueShowData> cues)
	{
		ClearCues();
		for (int i = 0; i < cues.Count; i++)
		{
			CueShowData cueShowData = cues[i];
			Cues.Add(new CueVM(cueShowData.Cue, cueShowData.SkillChecks, cueShowData.SoulMarkShifts, m_IsFirstCueInAllBookEvent));
			HistoryCues.Add(new CueVM(cueShowData.Cue, cueShowData.SkillChecks, cueShowData.SoulMarkShifts, m_IsFirstCueInAllBookEvent));
			m_IsFirstCueInAllBookEvent = false;
		}
	}

	private void ClearAnswers()
	{
		Answers.Value?.ForEach(delegate(AnswerVM d)
		{
			d.Dispose();
		});
		Answers.Value = null;
	}

	private void SetAnswers(List<BlueprintAnswer> answers)
	{
		ClearAnswers();
		int index = 1;
		Answers.Value = answers.Select((BlueprintAnswer answer) => new AnswerVM(answer, Game.Instance.DialogController, index++)).ToList();
	}

	private void SetPicture(BlueprintBookPage page)
	{
		EventPicture.Value = DefaultSprite;
		SpriteLink imageLink = page.ImageLink;
		if ((object)imageLink != null && imageLink.Exists())
		{
			Sprite sprite = page.ImageLink.Load();
			if (sprite != null && (bool)sprite)
			{
				EventPicture.Value = sprite;
			}
		}
	}

	private void SetMirror(BlueprintBookPage page)
	{
		Mirror.Value = Game.Instance.Player.MainCharacterEntity.Portrait.FullLengthPortrait;
	}

	public void HandleOnBookPageShow(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		SetPage(page, cues, answers);
	}

	public void HandleChooseCharacter(BlueprintAnswer answer)
	{
		AddDisposable(BookEventChooseCharacter = new BookEventChooseCharacterVM(answer));
		ChooseCharacterActive.Value = true;
	}

	public void HandleChooseCharacterEnd()
	{
		BookEventChooseCharacter?.Dispose();
		BookEventChooseCharacter = null;
		ChooseCharacterActive.Value = false;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			IsVisible.Value = false;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			IsVisible.Value = true;
		}
	}

	public void HandleDialogAnswerHover(string answer, bool hover)
	{
	}

	public void HandleDialogAnswerVote(NetPlayer player, string answer)
	{
		CheckVotesActive.Execute();
	}
}
