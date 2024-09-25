using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Toggles;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CombatLog;

public class CombatLogToggleWithCustomHint : OwlcatToggle
{
	[SerializeField]
	private FadeAnimator m_VotesHoverFadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_VotesHoverText;

	public void SetCustomHint(string hintText)
	{
		if (!(m_VotesHoverText == null))
		{
			m_VotesHoverText.text = hintText;
		}
	}
}
