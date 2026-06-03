using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickEntityHeaderView : TooltipBaseBrickView<TooltipBrickEntityHeaderVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MainTitle;

	[SerializeField]
	private GameObject m_ImageContainer;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_LeftLabel;

	[SerializeField]
	private TextMeshProUGUI m_RightLabel;

	[SerializeField]
	private TextMeshProUGUI m_RightLabelClassification;

	[SerializeField]
	private GameObject m_UpgradeItemIndicator;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_MainTitle, m_Title, m_LeftLabel, m_RightLabel, m_RightLabelClassification);
		}
		base.BindViewImplementation();
		m_MainTitle.text = base.ViewModel.MainTitle;
		if (base.ViewModel.IsAugment && base.ViewModel.Image == null)
		{
			m_ImageContainer.SetActive(value: true);
			m_Image.color = new Color(0f, 0f, 0f, 0f);
		}
		else
		{
			m_ImageContainer.SetActive(base.ViewModel.Image != null);
			m_Image.sprite = base.ViewModel.Image;
			m_Image.color = new Color(255f, 255f, 255f, 255f);
		}
		SetText(m_Title, base.ViewModel.Title);
		SetText(m_LeftLabel, base.ViewModel.LeftLabel);
		SetText(m_RightLabel, base.ViewModel.RightLabel);
		SetText(m_RightLabelClassification, base.ViewModel.RightLabelClassification);
		if ((bool)m_UpgradeItemIndicator)
		{
			m_UpgradeItemIndicator.SetActive(base.ViewModel.HasUpgrade);
		}
		m_TextHelper.UpdateTextSize();
		if (base.ViewModel.ItemTooltip != null)
		{
			this.SetTooltip(base.ViewModel.ItemTooltip);
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}

	private void SetText(TextMeshProUGUI textField, string text)
	{
		if (!(textField == null))
		{
			textField.gameObject.SetActive(!string.IsNullOrEmpty(text));
			textField.text = text;
		}
	}
}
