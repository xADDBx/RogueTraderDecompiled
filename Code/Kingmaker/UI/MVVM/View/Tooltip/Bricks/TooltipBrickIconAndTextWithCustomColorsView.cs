using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickIconAndTextWithCustomColorsView : TooltipBaseBrickView<TooltipBrickIconAndTextWithCustomColorsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_StringValue;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private float m_DefaultFontSize = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 22f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_StringValue.text = base.ViewModel.StringValue;
		m_Icon.sprite = base.ViewModel.Icon;
		m_StringValue.color = base.ViewModel.StringValueColor;
		m_Icon.color = base.ViewModel.IconColor;
		m_Background.color = base.ViewModel.BackgroundColor;
		m_StringValue.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
