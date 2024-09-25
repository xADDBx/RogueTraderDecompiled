using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.Controls.Selectable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickTwoColumnsStatView : TooltipBaseBrickView<TooltipBrickTwoColumnsStatVM>
{
	[SerializeField]
	private TextMeshProUGUI m_LabelLeft;

	[SerializeField]
	private TextMeshProUGUI m_LabelRight;

	[SerializeField]
	private TextMeshProUGUI m_ValueLeft;

	[SerializeField]
	private TextMeshProUGUI m_ValueRight;

	[SerializeField]
	private Image m_IconLeft;

	[SerializeField]
	private Image m_IconRight;

	[SerializeField]
	private GameObject m_IconContainerLeft;

	[SerializeField]
	private GameObject m_IconContainerRight;

	[SerializeField]
	private OwlcatMultiSelectable m_ComparisonLeft;

	[SerializeField]
	private OwlcatMultiSelectable m_ComparisonRight;

	[SerializeField]
	private Color m_RegularColor;

	[SerializeField]
	private Color m_HighlightColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_LabelLeft, m_LabelRight, m_ValueLeft, m_ValueRight);
		}
		m_LabelLeft.text = base.ViewModel.NameLeft;
		m_LabelRight.text = base.ViewModel.NameRight;
		m_ValueLeft.text = base.ViewModel.ValueLeft;
		m_ValueRight.text = base.ViewModel.ValueRight;
		m_IconLeft.sprite = base.ViewModel.IconLeft;
		m_IconRight.sprite = base.ViewModel.IconRight;
		m_IconContainerLeft.SetActive(base.ViewModel.IconLeft != null);
		m_IconContainerRight.SetActive(base.ViewModel.IconRight != null);
		m_ComparisonLeft.gameObject.SetActive(base.ViewModel.ComparisonLeft != ComparisonResult.Equal);
		m_ComparisonLeft.SetActiveLayer(base.ViewModel.ComparisonLeft.ToString());
		m_ComparisonRight.gameObject.SetActive(base.ViewModel.ComparisonRight != ComparisonResult.Equal);
		m_ComparisonRight.SetActiveLayer(base.ViewModel.ComparisonRight.ToString());
		m_ValueLeft.color = (base.ViewModel.HighlightLeft ? m_HighlightColor : m_RegularColor);
		m_ValueRight.color = (base.ViewModel.HighlightRight ? m_HighlightColor : m_RegularColor);
		m_TextHelper.UpdateTextSize();
		if (!float.IsNaN(base.ViewModel.NameSize))
		{
			m_LabelLeft.fontSize = base.ViewModel.NameSize;
			m_LabelRight.fontSize = base.ViewModel.NameSize;
		}
		if (!float.IsNaN(base.ViewModel.ValueSize))
		{
			m_ValueLeft.fontSize = base.ViewModel.ValueSize;
			m_ValueRight.fontSize = base.ViewModel.ValueSize;
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
