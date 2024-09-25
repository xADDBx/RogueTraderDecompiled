using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;

public class SpaceEventVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IDialogCueHandler, ISubscriber, IDialogHistoryHandler
{
	public readonly ReactiveCollection<IDialogShowData> History = new ReactiveCollection<IDialogShowData>();

	public readonly ReactiveProperty<List<AnswerVM>> Answers = new ReactiveProperty<List<AnswerVM>>(null);

	public readonly ReactiveProperty<AnswerVM> SystemAnswer = new ReactiveProperty<AnswerVM>(null);

	public readonly ReactiveProperty<CueVM> Cue = new ReactiveProperty<CueVM>(null);

	public readonly ReactiveCommand OnCueUpdate = new ReactiveCommand();

	public readonly DialogNotificationsVM DialogNotifications;

	public SpaceEventVM()
	{
		AddDisposable(DialogNotifications = new DialogNotificationsVM());
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
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
		BlueprintCue cue = data.Cue;
		if (!cue)
		{
			return;
		}
		DialogController dialogController = Game.Instance.DialogController;
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
		Cue.Value = new CueVM(cue, data.SkillChecks, data.SoulMarkShifts);
		VoiceOverPlayer.PlayVoiceOver(cue.Text);
		OnCueUpdate.Execute();
	}

	public void HandleOnDialogHistory(IDialogShowData showData)
	{
		History.Add(showData);
	}
}
