using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickDoubleTextView : TooltipBaseBrickView<TooltipBrickDoubleTextVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_LeftLabel;

	[SerializeField]
	protected GameObject m_LeftSide;

	[SerializeField]
	protected TextMeshProUGUI m_RightLabel;

	[SerializeField]
	protected GameObject m_RightSide;

	protected AccessibilityTextHelper TextHelper;

	protected override void BindViewImplementation()
	{
		if (TextHelper == null)
		{
			TextHelper = new AccessibilityTextHelper(m_LeftLabel, m_RightLabel);
		}
		m_LeftLabel.text = base.ViewModel.LeftLine;
		m_LeftSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.LeftLine));
		m_LeftSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.LeftAlignment;
		m_RightLabel.text = base.ViewModel.RightLine;
		m_RightSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.RightLine));
		m_RightSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.RightAlignment;
		TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		TextHelper.Dispose();
	}
}
