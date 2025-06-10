using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;

public class AnswerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBookEventUIHandler, ISubscriber, INetPingDialogAnswer
{
	public readonly IReactiveProperty<BlueprintAnswer> Answer = new ReactiveProperty<BlueprintAnswer>();

	public readonly IReactiveProperty<bool> Enable = new BoolReactiveProperty();

	public readonly ReactiveProperty<bool> WasChoose = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<TooltipBaseTemplate> AnswerTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly DialogVotesBlockVM DialogVotesBlockVM;

	public readonly int Index;

	private readonly DialogController m_DialogController;

	private bool m_ChooseCharacterInit;

	public readonly bool IsSystem;

	public static ReactiveProperty<string> ChoosedAnswer = new ReactiveProperty<string>();

	public readonly ReactiveCommand<List<NetPlayer>> VotedPlayersChanged = new ReactiveCommand<List<NetPlayer>>();

	public readonly List<NetPlayer> VotedPlayers = new List<NetPlayer>(new List<NetPlayer>());

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public AnswerVM(BlueprintAnswer answer, DialogController dialogController, int index)
	{
		Index = index;
		IsSystem = answer.IsSystem();
		m_DialogController = dialogController;
		m_ChooseCharacterInit = false;
		Enable.Value = answer.CanSelect();
		Answer.Value = answer;
		SetupTooltip();
		AddDisposable(DialogVotesBlockVM = new DialogVotesBlockVM());
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnChooseAnswer()
	{
		if (!Enable.Value || m_ChooseCharacterInit || !UINetUtility.IsControlMainCharacterWithWarning(needSignalHowToPing: true))
		{
			return;
		}
		if (Answer.Value.CharacterSelection.SelectionType == CharacterSelection.Type.Manual)
		{
			EventBus.RaiseEvent(delegate(IBookEventUIHandler h)
			{
				h.HandleChooseCharacter(Answer.Value);
			});
		}
		else if (m_DialogController != null)
		{
			Game.Instance.GameCommandQueue.DialogAnswer(m_DialogController.CurrentCueUpdateTick, Answer.Value.AssetGuid);
		}
		ChoosedAnswer.Value = Answer.Value.Text;
		WasChoose.Value = true;
	}

	public bool IsAlreadySelected()
	{
		return Game.Instance.Player.Dialog.SelectedAnswersContains(Answer.Value);
	}

	public void HandleChooseCharacter(BlueprintAnswer answer)
	{
		m_ChooseCharacterInit = true;
	}

	public void HandleChooseCharacterEnd()
	{
		m_ChooseCharacterInit = false;
	}

	public void PingAnswerHover(bool hover)
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			try
			{
				PhotonManager.Ping.PingDialogAnswer(Answer.Value.AssetGuid, hover);
			}
			catch (Exception arg)
			{
				PFLog.Net.Error($"Ping in dialog error {arg}");
				throw;
			}
		}
	}

	public void HandleDialogAnswerHover(string answer, bool hover)
	{
		if (!UINetUtility.IsControlMainCharacter() && !(answer != Answer.Value.AssetGuid))
		{
			WasChoose.Value = hover;
		}
	}

	public void HandleDialogAnswerVote(NetPlayer player, string answer)
	{
		if (answer != Answer.Value.AssetGuid)
		{
			if (VotedPlayers.Contains(player))
			{
				VotedPlayers.Remove(player);
				VotedPlayersChanged.Execute(VotedPlayers);
			}
		}
		else if (VotedPlayers.Contains(player))
		{
			VotedPlayers.Remove(player);
			VotedPlayersChanged.Execute(VotedPlayers);
		}
		else
		{
			UISounds.Instance.Sounds.Coop.DialogVotePing.Play();
			VotedPlayers.Add(player);
			VotedPlayersChanged.Execute(VotedPlayers);
		}
	}

	private void SetupTooltip()
	{
		if (Answer.Value.HasConditionsForTooltip && !Answer.Value.CanSelect())
		{
			AnswerTooltip.Value = new TooltipTemplateAnswerConditions(Answer.Value);
		}
		else if (Answer.Value.HasExchangeData)
		{
			AnswerTooltip.Value = new TooltipTemplateAnswerExchange(Answer.Value);
		}
		else if (Answer.Value.SkillChecksDC.Count > 0)
		{
			AnswerTooltip.Value = new TooltipTemplateSkillCheckDC(Answer.Value.SkillChecksDC);
		}
		else
		{
			AnswerTooltip.Value = null;
		}
	}
}
