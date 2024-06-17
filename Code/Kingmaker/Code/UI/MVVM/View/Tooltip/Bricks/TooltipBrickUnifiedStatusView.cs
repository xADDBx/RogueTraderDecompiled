using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Utility.DotNetExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickUnifiedStatusView : TooltipBaseBrickView<TooltipBrickUnifiedStatusVM>
{
	[Header("View Params")]
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private Image m_Frame;

	[Header("Unified Status Params")]
	[SerializeField]
	private UnifiedStatusParams[] UnifiedStatusParams;

	[SerializeField]
	private float m_DefaultFontSize = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 20f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = base.ViewModel.Text;
		m_Text.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		ApplyStatus(base.ViewModel.Status);
	}

	private void ApplyStatus(UnifiedStatus status)
	{
		UnifiedStatusParams paramsByStatus = GetParamsByStatus(status);
		if (paramsByStatus != null)
		{
			m_Text.color = paramsByStatus.TextColor;
			m_Image.sprite = paramsByStatus.Icon;
			m_Image.color = paramsByStatus.IconColor;
			m_Frame.color = paramsByStatus.FrameColor;
			m_Frame.gameObject.SetActive(paramsByStatus.ShowFrame);
		}
	}

	private UnifiedStatusParams GetParamsByStatus(UnifiedStatus status)
	{
		return UnifiedStatusParams.FirstItem((UnifiedStatusParams i) => i.Status == status);
	}
}
