using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.Utility;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.UI.Sound;
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
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public class DialogAnswerBaseView : ViewBase<AnswerVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, ISettingsFontSizeUIHandler, ISubscriber
{
	[SerializeField]
	protected TextMeshProUGUI m_AnswerText;

	[SerializeField]
	protected OwlcatMultiButton m_OwlcatButton;

	[Header("Tween Params")]
	[SerializeField]
	private float m_TweenDuration = 0.5f;

	[Space]
	[Header("VotesCoop")]
	[SerializeField]
	protected DialogVotesBlockView m_DialogVotesBlock;

	protected TooltipConfig TooltipConfig;

	private DialogColors m_DialogColors;

	protected FocusStateMachine Focus;

	private RectTransform m_AnswersTooltipPlace;

	public TextMeshProUGUI Text => m_AnswerText;

	public List<SkillCheckDC> SkillChecksDC => base.ViewModel.Answer.Value.SkillChecksDC;

	public List<PlayerInfo> AnswerVotes { get; private set; } = new List<PlayerInfo>();


	public DialogVotesBlockView DialogVotesBlock => m_DialogVotesBlock;

	public void Initialize(DialogColors dialogColors, RectTransform tooltipPlace = null)
	{
		m_DialogColors = dialogColors;
		m_AnswersTooltipPlace = tooltipPlace;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		base.gameObject.SetActive(value: true);
		Focus = new FocusStateMachine(OnFocusStateChanged);
		AddDisposable(base.ViewModel.Answer.Subscribe(SetAnswer));
		AddDisposable(m_AnswerText.SetLinkTooltip(base.ViewModel.Answer.Value.SkillChecksDC, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_AnswersTooltipPlace, 0, 0, 0, new List<Vector2>
		{
			new Vector2(0.5f, 0f)
		})));
		AddDisposable(base.ViewModel.WasChoose.Subscribe(delegate
		{
			UpdateView();
		}));
		m_DialogVotesBlock.Bind(base.ViewModel.DialogVotesBlockVM);
		AddDisposable(base.ViewModel.VotedPlayersChanged.Subscribe(CheckCoopPlayersVotes));
		CheckCoopPlayersVotes(base.ViewModel.VotedPlayers);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		WidgetFactory.DisposeWidget(this);
	}

	public virtual void SetTooltipConfig(TooltipConfig config)
	{
		TooltipConfig = config;
	}

	private void SetAnswer(BlueprintAnswer answer)
	{
		m_DialogVotesBlock.Or(null)?.ShowHideHover(state: false);
		DialogType type = Game.Instance.DialogController.Dialog.Type;
		string text = $"DialogChoice{base.ViewModel.Index}";
		SetAnswerText((type == DialogType.Epilog) ? answer.DisplayText : UIConstsExtensions.GetAnswerString(answer, text, base.ViewModel.Index));
		Color32 color = m_DialogColors.NormalAnswer;
		if (type == DialogType.Common || type == DialogType.StarSystemEvent)
		{
			color = ((!answer.CanSelect()) ? m_DialogColors.DisabledAnswer : ((base.ViewModel.IsAlreadySelected() && !base.ViewModel.IsSystem) ? m_DialogColors.SelectedAnswer : m_DialogColors.NormalAnswer));
			if (answer.HasConditionsForTooltip && !answer.CanSelect())
			{
				string arg = "UI_Dialog_ConditionFail";
				string text2 = string.Format(UIConfig.Instance.UIDialogConditionsLinkFormat, answer.AssetGuid, arg);
				SetAnswerText(string.Format(UIDialog.Instance.AnswerDialogueFormat, base.ViewModel.Index, text2 + " " + answer.DisplayText));
			}
		}
		m_AnswerText.color = color;
		AddDisposable(Game.Instance.Keyboard.Bind(text, Confirm));
		if (base.ViewModel.IsSystem)
		{
			AddDisposable(Game.Instance.Keyboard.Bind("NextOrEnd", Confirm));
		}
	}

	private void SetAnswerText(string text)
	{
		m_AnswerText.text = text;
	}

	[Obsolete("Use SetHasOverlay(!value) instead")]
	public void ShowAnswerHint(bool invert)
	{
		SetHasOverlay(!invert);
	}

	public void SetHasOverlay(bool value)
	{
		Focus.SetHasOverlay(value);
	}

	protected virtual void OnFocusStateChanged(FocusState prev, FocusState curr)
	{
		UpdateView();
		base.ViewModel?.PingAnswerHover(curr != FocusState.None);
	}

	protected virtual void UpdateView()
	{
		FocusState focusState = Focus.State;
		if (base.ViewModel != null && base.ViewModel.WasChoose.Value)
		{
			focusState = FocusState.Foreground;
		}
		UpdateFontColor(focusState != FocusState.None);
		UpdateButtonLayer(focusState);
	}

	protected virtual void UpdateButtonLayer(FocusState state)
	{
		switch (state)
		{
		case FocusState.None:
		case FocusState.Background:
			m_OwlcatButton.SetActiveLayer("Normal");
			break;
		case FocusState.Foreground:
			m_OwlcatButton.SetActiveLayer("Focus");
			break;
		}
	}

	protected virtual void UpdateFontColor(bool hasAnyFocus)
	{
		if (base.ViewModel == null)
		{
			m_AnswerText.color = (hasAnyFocus ? m_DialogColors.FocusAnswer : m_DialogColors.NormalAnswer);
		}
		else
		{
			m_AnswerText.color = ((!hasAnyFocus) ? ((!base.ViewModel.Answer.Value.CanSelect()) ? m_DialogColors.DisabledAnswer : ((base.ViewModel.IsAlreadySelected() && !base.ViewModel.IsSystem) ? m_DialogColors.SelectedAnswer : m_DialogColors.NormalAnswer)) : (base.ViewModel.Answer.Value.CanSelect() ? m_DialogColors.FocusAnswer : m_DialogColors.FocusDisableAnswer));
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

	public virtual void UpdateTextSize(float multiplier)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	void IConsoleNavigationEntity.SetFocus(bool value)
	{
		Focus.SetHasFocus(value);
	}

	bool IConsoleNavigationEntity.IsValid()
	{
		return base.ViewModel != null;
	}

	bool IConfirmClickHandler.CanConfirmClick()
	{
		return m_OwlcatButton.CanConfirmClick();
	}

	void IConfirmClickHandler.OnConfirmClick()
	{
		Confirm();
	}

	string IConfirmClickHandler.GetConfirmClickHint()
	{
		return string.Empty;
	}

	void ISettingsFontSizeUIHandler.HandleChangeFontSizeSettings(float size)
	{
		UpdateTextSize(size);
	}

	public void Confirm()
	{
		if (!PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingDialogAnswerVote(base.ViewModel.Answer.Value.AssetGuid);
		}))
		{
			UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
			DelayedInvoker.InvokeInFrames(delegate
			{
				base.ViewModel?.OnChooseAnswer();
			}, 1);
		}
	}
}
