using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.Utility.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public abstract class TooltipBrickCombatLogBaseView<T> : TooltipBaseBrickView<T> where T : TooltipBrickCombatLogBaseVM
{
	[SerializeField]
	protected Image m_BackgroundImage;

	[SerializeField]
	protected Image m_IconImage;

	[SerializeField]
	protected TextMeshProUGUI m_NameText;

	[Header("Result Value")]
	[SerializeField]
	protected TextMeshProUGUI m_ResultValueText;

	[SerializeField]
	protected GameObject m_ResultLineImage;

	[Header("Nested Blocks")]
	[SerializeField]
	protected GameObject m_FirstNestedBlock;

	[SerializeField]
	protected GameObject m_SecondNestedBlock;

	[SerializeField]
	protected GameObject m_ThirdNestedBlock;

	[SerializeField]
	protected GameObject m_FourthNestedBlock;

	[Header("Sprites")]
	[SerializeField]
	protected Sprite m_ProtectionSprite;

	[SerializeField]
	protected Sprite m_TargetHitSprite;

	[SerializeField]
	protected Sprite m_BorderChanceSprite;

	[Header("Colors")]
	[Space]
	[SerializeField]
	protected Color m_GrayBackgroundColor;

	[SerializeField]
	protected Color m_BeigeBackgroundColor;

	[SerializeField]
	protected Color m_RedBackgroundColor;

	protected AccessibilityTextHelper TextHelper;

	protected override void BindViewImplementation()
	{
		if (TextHelper == null)
		{
			TextHelper = new AccessibilityTextHelper(m_NameText, m_ResultValueText);
		}
		m_NameText.text = base.ViewModel.Name;
		m_FirstNestedBlock.SetActive(base.ViewModel.NestedLevel > 0);
		m_SecondNestedBlock.SetActive(base.ViewModel.NestedLevel > 1);
		m_ThirdNestedBlock.SetActive(base.ViewModel.NestedLevel > 2);
		m_FourthNestedBlock.SetActive(base.ViewModel.NestedLevel > 3);
		if (base.ViewModel.IsProtectionIcon)
		{
			m_IconImage.sprite = m_ProtectionSprite;
		}
		else if (base.ViewModel.IsTargetHitIcon)
		{
			m_IconImage.sprite = m_TargetHitSprite;
		}
		else if (base.ViewModel.IsBorderChanceIcon)
		{
			m_IconImage.sprite = m_BorderChanceSprite;
		}
		m_IconImage.gameObject.SetActive(base.ViewModel.IsProtectionIcon || base.ViewModel.IsTargetHitIcon || base.ViewModel.IsBorderChanceIcon);
		Color color = Color.clear;
		if (base.ViewModel.IsGrayBackground)
		{
			color = m_GrayBackgroundColor;
		}
		else if (base.ViewModel.IsBeigeBackground)
		{
			color = m_BeigeBackgroundColor;
		}
		else if (base.ViewModel.IsRedBackground)
		{
			color = m_RedBackgroundColor;
		}
		m_BackgroundImage.color = color;
		m_ResultValueText.text = base.ViewModel.ResultValue;
		m_ResultValueText.gameObject.SetActive(base.ViewModel.IsResultValue && !base.ViewModel.ResultValue.IsNullOrEmpty());
		m_ResultLineImage.SetActive(base.ViewModel.IsResultValue && base.ViewModel.ResultValue.IsNullOrEmpty());
		TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		TextHelper.Dispose();
	}
}
