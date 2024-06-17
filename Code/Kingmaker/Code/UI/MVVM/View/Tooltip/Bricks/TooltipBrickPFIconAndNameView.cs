using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPFIconAndNameView : TooltipBaseBrickView<TooltipBrickPFIconAndNameVM>
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	private float m_DefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Label;
		m_Title.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}
}
