using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickIconValueStatView : TooltipBaseBrickView<TooltipBrickIconValueStatVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Color m_LabelColor;

	[SerializeField]
	private Color m_LabelSecondaryColor;

	[SerializeField]
	private Color m_ValueColor;

	[Header("Size")]
	[SerializeField]
	private int m_NormalSize = 30;

	[SerializeField]
	private int m_SmallSize = 24;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private LayoutElement m_IconContainer;

	[SerializeField]
	private HorizontalLayoutGroup m_LayoutGroup;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label, m_Value);
		}
		m_Label.text = base.ViewModel.Name;
		m_Value.text = base.ViewModel.Value;
		m_Icon.sprite = base.ViewModel.Icon;
		if (base.ViewModel.IsIconWhite)
		{
			m_Icon.color = Color.white;
		}
		m_IconContainer.gameObject.SetActive(base.ViewModel.Icon != null);
		ApplyType();
		if (base.ViewModel.NeedChangeSize)
		{
			m_Label.enableAutoSizing = false;
			m_Value.enableAutoSizing = false;
			m_Label.fontSize = base.ViewModel.TextSize;
			m_Value.fontSize = base.ViewModel.ValueSize;
		}
		if (base.ViewModel.NeedChangeColor)
		{
			m_Label.color = base.ViewModel.NameTextColor;
			m_Value.color = base.ViewModel.ValueTextColor;
		}
		else
		{
			m_Label.color = (base.ViewModel.UseSecondaryLabelColor ? m_LabelSecondaryColor : m_LabelColor);
			m_Value.color = m_ValueColor;
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}

	private void ApplyType()
	{
		if (!(m_LayoutElement == null) && !(m_IconContainer == null) && !(m_LayoutGroup == null))
		{
			int num = m_NormalSize;
			TextAnchor childAlignment = TextAnchor.MiddleLeft;
			TextAlignmentOptions alignment = TextAlignmentOptions.Left;
			if (base.ViewModel.Type.HasFlag(TooltipIconValueStatType.Small))
			{
				num = m_SmallSize;
			}
			if (base.ViewModel.Type.HasFlag(TooltipIconValueStatType.Centered))
			{
				childAlignment = TextAnchor.MiddleCenter;
			}
			if (base.ViewModel.Type.HasFlag(TooltipIconValueStatType.Justified))
			{
				alignment = TextAlignmentOptions.Right;
			}
			if (base.ViewModel.Type.HasFlag(TooltipIconValueStatType.Inverted))
			{
				m_Label.text = base.ViewModel.Value;
				m_Value.text = base.ViewModel.Name;
				m_Label.color = m_ValueColor;
				m_Value.color = m_LabelColor;
			}
			if (base.ViewModel.Type.HasFlag(TooltipIconValueStatType.NameTextNormal))
			{
				m_Label.fontStyle = FontStyles.Normal;
			}
			if (base.ViewModel.Type.HasFlag(TooltipIconValueStatType.NameTextBold))
			{
				m_Label.fontStyle = FontStyles.Bold;
			}
			m_LayoutElement.minHeight = num;
			m_IconContainer.preferredWidth = num;
			m_Label.alignment = alignment;
			m_LayoutGroup.childAlignment = childAlignment;
		}
	}
}
