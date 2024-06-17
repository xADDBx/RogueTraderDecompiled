using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Dialog.Dialog;

public class DialogVotesBlockView : ViewBase<DialogVotesBlockVM>
{
	[SerializeField]
	protected OwlcatButton m_VotesBlock;

	[SerializeField]
	protected Image m_VotesOnePersonIcon;

	[SerializeField]
	protected TextMeshProUGUI m_VotesText;

	[SerializeField]
	protected FadeAnimator m_VotesHoverFadeAnimator;

	[SerializeField]
	protected TextMeshProUGUI m_VotesHoverText;

	public OwlcatButton FocusButton => m_VotesBlock;

	protected override void BindViewImplementation()
	{
		ShowHideHover(state: false);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ShowHideHover(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void CheckVotesPlayers(List<PlayerInfo> answerVotes)
	{
		if (!PhotonManager.Lobby.IsActive)
		{
			return;
		}
		bool flag = answerVotes.Any();
		base.gameObject.SetActive(flag);
		if (flag)
		{
			bool flag2 = answerVotes.Count == 1;
			m_VotesOnePersonIcon.transform.parent.gameObject.SetActive(flag2);
			m_VotesText.transform.parent.gameObject.SetActive(!flag2);
			List<string> names = new List<string>();
			answerVotes.ForEach(delegate(PlayerInfo p)
			{
				names.Add((PhotonManager.Player.GetNickName(p.Player, out var nickName) && !string.IsNullOrWhiteSpace(nickName)) ? nickName : p.UserId);
			});
			if (m_VotesHoverText != null)
			{
				m_VotesHoverText.text = string.Join(Environment.NewLine, names);
			}
			if (flag2)
			{
				m_VotesOnePersonIcon.sprite = answerVotes.FirstOrDefault().Player.GetPlayerIcon();
			}
			else
			{
				m_VotesText.text = answerVotes.Count.ToString();
			}
		}
	}
}
