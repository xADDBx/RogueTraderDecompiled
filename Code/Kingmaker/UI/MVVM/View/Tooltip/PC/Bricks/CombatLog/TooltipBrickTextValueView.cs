using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickTextValueView : TooltipBaseBrickView<TooltipBrickTextValueVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	protected GameObject m_ResultLineImage;

	[Header("Nested Blocks")]
	[SerializeField]
	protected GameObject m_FirstNestedBlock;

	[SerializeField]
	protected GameObject m_SecondNestedBlock;

	[SerializeField]
	protected GameObject m_ThirdNestedBlock;

	[SerializeField]
	protected GameObject m_FourthNestedBlock;

	[SerializeField]
	protected Image m_Line;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text, m_Value);
		}
		m_Text.text = base.ViewModel.Text;
		m_Value.text = base.ViewModel.Value;
		m_FirstNestedBlock.SetActive(base.ViewModel.NestedLevel > 0);
		m_SecondNestedBlock.SetActive(base.ViewModel.NestedLevel > 1);
		m_ThirdNestedBlock.SetActive(base.ViewModel.NestedLevel > 2);
		m_FourthNestedBlock.SetActive(base.ViewModel.NestedLevel > 3);
		m_ResultLineImage.SetActive(base.ViewModel.IsResultValue);
		m_Line.gameObject.SetActive(base.ViewModel.NeedShowLine);
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
