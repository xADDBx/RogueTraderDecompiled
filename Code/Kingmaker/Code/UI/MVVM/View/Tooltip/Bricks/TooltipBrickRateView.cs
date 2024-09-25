using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickRateView : TooltipBaseBrickView<TooltipBrickRateVM>
{
	[SerializeField]
	private TextMeshProUGUI m_RateTitle;

	[SerializeField]
	private List<GameObject> m_RateSlots;

	[SerializeField]
	private List<GameObject> m_RatePoints;

	[SerializeField]
	private float m_DefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_RateTitle.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.RateName));
		m_RateTitle.text = ((!string.IsNullOrEmpty(base.ViewModel.RateName)) ? base.ViewModel.RateName : string.Empty);
		m_RateTitle.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		for (int i = 0; i < m_RateSlots.Count; i++)
		{
			m_RateSlots[i].SetActive(i < base.ViewModel.MaxRate);
		}
		for (int j = 0; j < m_RatePoints.Count; j++)
		{
			m_RatePoints[j].SetActive(j < base.ViewModel.Rate);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
