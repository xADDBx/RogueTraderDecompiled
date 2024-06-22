using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickAnomalyInfoView : TooltipBaseBrickView<TooltipBrickAnomalyInfoVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private float m_DefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = base.ViewModel.Name;
		m_Text.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
