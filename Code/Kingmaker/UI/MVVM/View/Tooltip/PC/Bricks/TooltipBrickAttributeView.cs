using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickAttributeView : TooltipBaseBrickView<TooltipBrickAttributeVM>
{
	[Serializable]
	private class StripeView
	{
		public GameObject Container;

		public TextMeshProUGUI Acronym;
	}

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private GameObject m_RecommendedMark;

	[SerializeField]
	private StripeView m_StatStripe;

	[SerializeField]
	private StripeView m_SkillStripe;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	private List<StripeView> Stripes => new List<StripeView> { m_StatStripe, m_SkillStripe };

	protected override void BindViewImplementation()
	{
		foreach (StripeView stripe2 in Stripes)
		{
			stripe2.Container.SetActive(value: false);
		}
		m_Label.text = base.ViewModel.Name;
		StripeView stripe = GetStripe(base.ViewModel.StripeType);
		stripe.Container.SetActive(value: true);
		stripe.Acronym.text = base.ViewModel.Acronym;
		m_RecommendedMark.SetActive(base.ViewModel.IsRecommended);
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
		if (m_AccessibilityTextHelper == null)
		{
			m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Label);
		}
		m_AccessibilityTextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_AccessibilityTextHelper.Dispose();
		m_AccessibilityTextHelper = null;
	}

	private StripeView GetStripe(StripeType type)
	{
		return type switch
		{
			StripeType.Skill => m_SkillStripe, 
			StripeType.Stat => m_StatStripe, 
			_ => m_StatStripe, 
		};
	}
}
