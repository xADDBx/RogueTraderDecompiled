using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.Utility;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Player;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

public class BookEventAnswerView : ViewBase<AnswerVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	protected TextMeshProUGUI m_AnswerText;

	[SerializeField]
	protected OwlcatMultiButton m_OwlcatButton;

	[Space]
	[Header("VotesCoop")]
	[SerializeField]
	protected DialogVotesBlockView m_DialogVotesBlock;

	private BookEventBaseView m_BookEventView;

	public TextMeshProUGUI Text => m_AnswerText;

	public List<SkillCheckDC> SkillChecksDC => base.ViewModel?.Answer?.Value?.SkillChecksDC;

	public List<PlayerInfo> AnswerVotes { get; private set; } = new List<PlayerInfo>();


	public DialogVotesBlockView DialogVotesBlock => m_DialogVotesBlock;

	public OwlcatMultiButton Button => m_OwlcatButton;

	public void Initialize(BookEventBaseView bookEventBaseView)
	{
		m_BookEventView = bookEventBaseView;
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.Answer.Subscribe(SetAnswer));
		AddDisposable(m_AnswerText.SetLinkTooltip(base.ViewModel.Answer.Value.SkillChecksDC, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, null, 0, 0, 420)));
		m_DialogVotesBlock.Bind(base.ViewModel.DialogVotesBlockVM);
		AddDisposable(base.ViewModel.VotedPlayersChanged.Subscribe(CheckCoopPlayersVotes));
		CheckCoopPlayersVotes(base.ViewModel.VotedPlayers);
		AddDisposable(base.ViewModel.WasChoose.Subscribe(m_OwlcatButton.SetFocus));
		AddDisposable(base.ViewModel.Enable.Subscribe(m_OwlcatButton.SetInteractable));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		WidgetFactory.DisposeWidget(this);
	}

	protected void SetAnswer(BlueprintAnswer answer)
	{
		m_DialogVotesBlock.Or(null)?.ShowHideHover(state: false);
		DialogType type = Game.Instance.DialogController.Dialog.Type;
		string text = $"DialogChoice{base.ViewModel.Index}";
		SetAnswerText((type == DialogType.Epilog) ? answer.DisplayText : UIConstsExtensions.GetAnswerFormattedString(answer, text, base.ViewModel.Index));
		if ((type == DialogType.Common || type == DialogType.StarSystemEvent) && answer.SelectConditions.HasConditions)
		{
			string text2 = string.Empty;
			Condition[] conditions = answer.SelectConditions.Conditions;
			foreach (Condition condition in conditions)
			{
				if (condition is ConditionHaveFullCargo && !condition.Not)
				{
					text2 += string.Format(UIDialog.Instance.AnswerYouNeedFullCargo, condition.GetCaption());
				}
				else if (condition is ContextConditionHasItem && !condition.Not)
				{
					text2 += string.Format(UIDialog.Instance.AnswerYouNeedItem, condition.GetCaption());
				}
			}
			SetAnswerText(string.Format(UIDialog.Instance.AnswerDialogueFormat, base.ViewModel.Index, text2 + " " + answer.DisplayText));
		}
		AddDisposable(Game.Instance.Keyboard.Bind(text, Confirm));
		if (base.ViewModel.IsSystem)
		{
			AddDisposable(Game.Instance.Keyboard.Bind("NextOrEnd", Confirm));
		}
	}

	private void CheckCoopPlayersVotes(List<NetPlayer> players)
	{
		if (players == null || m_DialogVotesBlock == null || PhotonManager.Instance == null)
		{
			return;
		}
		ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
		AnswerVotes.Clear();
		AnswerVotes = players.Select((NetPlayer player) => activePlayers.FirstOrDefault((PlayerInfo p) => player.Index == p.NetPlayer.Index)).ToList();
		m_DialogVotesBlock.CheckVotesPlayers(AnswerVotes);
	}

	private void SetAnswerText(string text)
	{
		m_AnswerText.text = text;
	}

	public void Confirm()
	{
		if (m_BookEventView.IsShowHistory.Value)
		{
			return;
		}
		if (PhotonManager.NetGame.CurrentState == NetGame.State.Playing && !UINetUtility.IsControlMainCharacter())
		{
			PhotonManager.Ping.PressPing(delegate
			{
				PhotonManager.Ping.PingDialogAnswerVote(base.ViewModel.Answer.Value.AssetGuid);
			});
			return;
		}
		UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
		DelayedInvoker.InvokeInFrames(delegate
		{
			base.ViewModel?.OnChooseAnswer();
		}, 1);
	}

	public virtual void SetFocus(bool value)
	{
		AnswerVM viewModel = base.ViewModel;
		int num;
		if (viewModel != null)
		{
			ReactiveProperty<bool> wasChoose = viewModel.WasChoose;
			if (wasChoose != null)
			{
				num = (wasChoose.Value ? 1 : 0);
				goto IL_001d;
			}
		}
		num = 0;
		goto IL_001d;
		IL_001d:
		bool flag = (byte)num != 0;
		m_OwlcatButton.Or(null)?.SetFocus(value || flag);
		base.ViewModel?.PingAnswerHover(value);
	}

	public void OnPointerEnter()
	{
		m_BookEventView.SetAnswerFocusTo(this);
	}

	public bool IsValid()
	{
		return base.ViewModel != null;
	}

	public bool CanConfirmClick()
	{
		return m_OwlcatButton.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		Confirm();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
