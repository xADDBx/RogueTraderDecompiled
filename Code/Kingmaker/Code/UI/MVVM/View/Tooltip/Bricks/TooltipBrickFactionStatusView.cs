using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickFactionStatusView : TooltipBaseBrickView<TooltipBrickFactionStatusVM>
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	protected TextMeshProUGUI m_Status;

	[SerializeField]
	private float m_DefaultFontSizeLabel = 18f;

	[SerializeField]
	private float m_DefaultFontSizeStatus = 13f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeLabel = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeStatus = 13f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Label.text = base.ViewModel.Label;
		m_Status.text = base.ViewModel.Status;
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Label.fontSize = (isControllerMouse ? m_DefaultFontSizeLabel : m_DefaultConsoleFontSizeLabel) * FontMultiplier;
		m_Status.fontSize = (isControllerMouse ? m_DefaultFontSizeStatus : m_DefaultConsoleFontSizeStatus) * FontMultiplier;
	}
}
