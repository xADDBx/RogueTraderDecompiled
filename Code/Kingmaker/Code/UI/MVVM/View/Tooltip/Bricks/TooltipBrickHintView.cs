using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickHintView : TooltipBaseBrickView<TooltipBrickHintVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = base.ViewModel.Text;
		m_Text.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}
}
