using JetBrains.Annotations;
using Kingmaker.UI.Sound;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public class InitiativeTrackerEndOfRound : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_RoundText;

	private int m_PreviousRound;

	public RectTransform RectTransform => base.gameObject.transform as RectTransform;

	public void SetRound(int round)
	{
		if (round == 0)
		{
			m_RoundText.text = "S";
			return;
		}
		m_RoundText.text = $"{round}";
		if (m_PreviousRound != round)
		{
			UISounds.Instance.Sounds.InitiativeTracker.InitiativeTrackerRoundCount.Play();
		}
		m_PreviousRound = round;
	}

	public void Initialize()
	{
		m_RoundText.text = "";
	}
}
