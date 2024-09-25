using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPortraitFeaturesView : TooltipBaseBrickView<TooltipBrickPortraitFeaturesVM>
{
	[FormerlySerializedAs("m_Name")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private TextMeshProUGUI m_Available;

	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	[SerializeField]
	private List<Image> m_DesperateMeasureIcons;

	[SerializeField]
	private List<Image> m_HeroicActIcons;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_NameLabel, m_Available);
		}
		m_NameLabel.text = base.ViewModel.Name;
		m_Available.text = base.ViewModel.AvailableText;
		m_Portrait.sprite = base.ViewModel.Portrait;
		m_GrayScale.EffectAmount = ((!base.ViewModel.Available) ? 1 : 0);
		for (int i = 0; i < base.ViewModel.DesperateMeasureAbilities.Count; i++)
		{
			if (base.ViewModel.DesperateMeasureAbilities[i] != null)
			{
				m_DesperateMeasureIcons[i].sprite = base.ViewModel.DesperateMeasureAbilities[i].Icon;
				m_DesperateMeasureIcons[i].gameObject.SetActive(value: true);
				AddDisposable(m_DesperateMeasureIcons[i].SetTooltip(new TooltipTemplateAbility(base.ViewModel.DesperateMeasureAbilities[i].Data)));
			}
		}
		for (int j = 0; j < base.ViewModel.HeroicActAbilities.Count; j++)
		{
			if (base.ViewModel.HeroicActAbilities[j] != null)
			{
				m_HeroicActIcons[j].sprite = base.ViewModel.HeroicActAbilities[j].Icon;
				m_HeroicActIcons[j].gameObject.SetActive(value: true);
				AddDisposable(m_HeroicActIcons[j].SetTooltip(new TooltipTemplateAbility(base.ViewModel.HeroicActAbilities[j].Data)));
			}
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
