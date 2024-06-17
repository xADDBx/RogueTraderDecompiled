using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickMomentumPortraitView : TooltipBaseBrickView<TooltipBrickMomentumPortraitVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private string m_EnableText;

	[SerializeField]
	private string m_DisableText;

	[SerializeField]
	private Color m_EnableTextColor;

	[SerializeField]
	private Color m_DisableTextColor;

	protected override void BindViewImplementation()
	{
		m_Icon.sprite = base.ViewModel.Sprite;
		m_GrayScale.EffectAmount = ((!base.ViewModel.Enable) ? 1 : 0);
		m_Text.color = (base.ViewModel.Enable ? m_EnableTextColor : m_DisableTextColor);
		m_Text.text = (base.ViewModel.Enable ? m_EnableText : m_DisableText);
	}
}
