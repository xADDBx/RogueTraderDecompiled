using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickTextBackgroundView : TooltipBaseBrickView<TooltipBrickTextBackgroundVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	protected Image m_BackgroundImage;

	[SerializeField]
	private GameObject m_RightDecoration;

	[SerializeField]
	private GameObject m_LeftDecoration;

	[SerializeField]
	private LayoutGroup m_LayoutGroup;

	[SerializeField]
	private Color m_DefaultColor = Color.black;

	[SerializeField]
	private Color m_BrightColor = Color.white;

	[SerializeField]
	private float m_DefaultFontSize = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 22f;

	[Header("Colors")]
	[Space]
	[SerializeField]
	protected Color m_GrayBackgroundColor;

	[SerializeField]
	protected Color m_GreenBackgroundColor;

	[SerializeField]
	protected Color m_RedBackgroundColor;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = base.ViewModel.Text;
		if ((bool)m_RightDecoration && (bool)m_LeftDecoration)
		{
			m_RightDecoration.SetActive(base.ViewModel.IsHeader);
			m_LeftDecoration.SetActive(base.ViewModel.IsHeader);
		}
		if ((bool)m_LayoutGroup)
		{
			ApplyAlignment(base.ViewModel.Alignment);
		}
		ApplyStyle(base.ViewModel.Type);
		if (base.ViewModel.NeedChangeSize)
		{
			m_Text.enableAutoSizing = false;
			m_Text.fontSize = (float)base.ViewModel.TextSize * FontMultiplier;
		}
		Color color = Color.clear;
		if (base.ViewModel.IsGrayBackground)
		{
			color = m_GrayBackgroundColor;
		}
		else if (base.ViewModel.IsGreenBackground)
		{
			color = m_GreenBackgroundColor;
		}
		else if (base.ViewModel.IsRedBackground)
		{
			color = m_RedBackgroundColor;
		}
		m_BackgroundImage.color = color;
		AddDisposable(m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void ApplyStyle(TooltipTextType type)
	{
		ApplyStyleTo(m_Text, type);
	}

	protected void ApplyStyleTo(TextMeshProUGUI text, TooltipTextType type)
	{
		text.paragraphSpacing = 0f;
		text.fontStyle = FontStyles.Normal;
		text.alignment = TextAlignmentOptions.Left;
		text.color = m_DefaultColor;
		if (type.HasFlag(TooltipTextType.Paragraph))
		{
			text.paragraphSpacing = 60f;
		}
		if (type.HasFlag(TooltipTextType.Italic))
		{
			text.fontStyle = FontStyles.Italic;
		}
		if (type.HasFlag(TooltipTextType.Bold))
		{
			text.fontStyle = FontStyles.Bold;
		}
		if (type.HasFlag(TooltipTextType.Centered))
		{
			text.alignment = TextAlignmentOptions.Center;
		}
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		if (type.HasFlag(TooltipTextType.GlossarySize))
		{
			text.fontSize = 20f * FontMultiplier;
		}
		else if (!type.HasFlag(TooltipTextType.GlossarySize))
		{
			text.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		}
		if (type.HasFlag(TooltipTextType.BlackColor))
		{
			text.color = Color.black;
		}
		else if (type.HasFlag(TooltipTextType.BrightColor))
		{
			text.color = m_BrightColor;
		}
	}

	private void ApplyAlignment(TooltipTextAlignment alignment)
	{
		switch (alignment)
		{
		case TooltipTextAlignment.Right:
			m_LayoutGroup.childAlignment = TextAnchor.MiddleRight;
			m_Text.verticalAlignment = VerticalAlignmentOptions.Middle;
			m_Text.horizontalAlignment = HorizontalAlignmentOptions.Right;
			break;
		case TooltipTextAlignment.Midl:
			m_LayoutGroup.childAlignment = TextAnchor.MiddleCenter;
			m_Text.verticalAlignment = VerticalAlignmentOptions.Middle;
			m_Text.horizontalAlignment = HorizontalAlignmentOptions.Center;
			break;
		case TooltipTextAlignment.Left:
			m_LayoutGroup.childAlignment = TextAnchor.MiddleLeft;
			m_Text.verticalAlignment = VerticalAlignmentOptions.Middle;
			m_Text.horizontalAlignment = HorizontalAlignmentOptions.Center;
			break;
		}
	}
}
